using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using LibVLCSharp.Shared;
using TvApp.Models;
using TvApp.Services;

namespace TvApp.ViewModels;

public class MainViewModel : ViewModelBase, IDisposable
{
    private const string FavoritesCategoryId = "__favorites__";

    private readonly XtreamAccount _account;
    private readonly XtreamService _xtreamService;
    private readonly FavoritesStore _favoritesStore;

    private readonly LibVLC _libVlc;
    private readonly MediaPlayer _mediaPlayer;

    private List<Channel> _allChannels = new();
    private HashSet<int> _favoriteIds = new();

    private ChannelCategory? _selectedCategory;
    private Channel? _selectedChannel;
    private Channel? _nowPlaying;
    private string _searchText = string.Empty;
    private bool _isLoading;
    private string? _errorMessage;
    private bool _isMuted;
    private int _volume = 80;
    private string? _lastAttemptedUrl;

    public MainViewModel(XtreamAccount account, XtreamService xtreamService, FavoritesStore favoritesStore)
    {
        _account = account;
        _xtreamService = xtreamService;
        _favoritesStore = favoritesStore;

        // "--verbose=2" ajuda a diagnosticar problemas de playback.
        // "--avcodec-hw=none" desativa decodificacao por hardware: evita a tela branca/preta
        // que ocorre em algumas maquinas (drivers de video, maquinas virtuais, Area de
        // Trabalho Remota) quando o LibVLC tenta usar Direct3D11 para o video.
        // "--vout=wingdi" forca a saida de video por GDI (sem aceleracao 3D), que e o modo
        // mais compativel possivel - resolve a tela branca ao custo de um pouco de desempenho.
        _libVlc = new LibVLC(enableDebugLogs: true, "--verbose=2", "--avcodec-hw=none", "--vout=wingdi");
        _mediaPlayer = new MediaPlayer(_libVlc) { Volume = _volume };

        _mediaPlayer.EncounteredError += OnEncounteredError;
        _mediaPlayer.Playing += OnPlaying;
        _mediaPlayer.Buffering += OnBuffering;

        Categories = new ObservableCollection<ChannelCategory>();
        Channels = new ObservableCollection<Channel>();

        RefreshCommand = new AsyncRelayCommand(LoadDataAsync, () => !IsLoading);
        PlayChannelCommand = new RelayCommand(p => PlayChannel(p as Channel), p => p is Channel);
        ToggleFavoriteCommand = new RelayCommand(p => ToggleFavorite(p as Channel), p => p is Channel);
        StopCommand = new RelayCommand(_ => Stop(), _ => NowPlaying != null);
        ToggleMuteCommand = new RelayCommand(_ => IsMuted = !IsMuted);
        LogoutCommand = new RelayCommand(_ => RequestLogout?.Invoke());

        AccountLabel = $"{account.Username} @ {account.NormalizedServerUrl}";
    }

    public string AccountLabel { get; }

    public ObservableCollection<ChannelCategory> Categories { get; }

    public ObservableCollection<Channel> Channels { get; }

    public MediaPlayer MediaPlayer => _mediaPlayer;

    public ChannelCategory? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetField(ref _selectedCategory, value))
                ApplyFilter();
        }
    }

    public Channel? SelectedChannel
    {
        get => _selectedChannel;
        set => SetField(ref _selectedChannel, value);
    }

    public Channel? NowPlaying
    {
        get => _nowPlaying;
        private set
        {
            if (SetField(ref _nowPlaying, value))
                OnPropertyChanged(nameof(NowPlayingLabel));
        }
    }

    public string NowPlayingLabel => NowPlaying is null ? "Nenhum canal em reproducao" : $"Reproduzindo: {NowPlaying.Name}";

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetField(ref _searchText, value))
                ApplyFilter();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetField(ref _isLoading, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetField(ref _errorMessage, value);
    }

    public bool IsMuted
    {
        get => _isMuted;
        set
        {
            if (SetField(ref _isMuted, value))
                _mediaPlayer.Mute = value;
        }
    }

    public int Volume
    {
        get => _volume;
        set
        {
            if (SetField(ref _volume, value))
                _mediaPlayer.Volume = value;
        }
    }

    public ICommand RefreshCommand { get; }
    public ICommand PlayChannelCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand ToggleMuteCommand { get; }
    public ICommand LogoutCommand { get; }

    public event Action? RequestLogout;

    public async Task InitializeAsync() => await LoadDataAsync();

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var categoriesTask = _xtreamService.GetLiveCategoriesAsync(_account);
            var channelsTask = _xtreamService.GetLiveStreamsAsync(_account);

            await Task.WhenAll(categoriesTask, channelsTask);

            var categories = categoriesTask.Result;
            _allChannels = channelsTask.Result;

            _favoriteIds = _favoritesStore.Load(_account);
            foreach (var channel in _allChannels)
                channel.IsFavorite = _favoriteIds.Contains(channel.StreamId);

            Categories.Clear();
            Categories.Add(new ChannelCategory
            {
                CategoryId = FavoritesCategoryId,
                CategoryName = "⭐ Favoritos",
                IsFavoritesPseudoGroup = true
            });
            foreach (var category in categories.OrderBy(c => c.CategoryName, StringComparer.OrdinalIgnoreCase))
                Categories.Add(category);

            SelectedCategory = Categories.FirstOrDefault(c => !c.IsFavoritesPseudoGroup) ?? Categories.FirstOrDefault();
        }
        catch (XtreamException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar canais: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFilter()
    {
        Channels.Clear();

        IEnumerable<Channel> source = SelectedCategory switch
        {
            null => Enumerable.Empty<Channel>(),
            { IsFavoritesPseudoGroup: true } => _allChannels.Where(c => c.IsFavorite),
            var cat => _allChannels.Where(c => c.CategoryId == cat.CategoryId)
        };

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            source = source.Where(c => c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var channel in source.OrderBy(c => c.Num))
            Channels.Add(channel);
    }

    private void PlayChannel(Channel? channel)
    {
        if (channel is null) return;

        ErrorMessage = null;

        var url = XtreamService.BuildLiveStreamUrl(_account, channel);
        _lastAttemptedUrl = url;

        try
        {
            var media = new Media(_libVlc, url, FromType.FromLocation);
            // Aumenta a tolerancia de buffer de rede: streams IPTV via HTTP costumam
            // precisar de mais cache do que o padrao do LibVLC para nao "morrer" logo no inicio.
            media.AddOption(":network-caching=3000");
            media.AddOption(":live-caching=3000");

            var started = _mediaPlayer.Play(media);
            media.Dispose();

            if (!started)
            {
                ErrorMessage = $"Nao foi possivel iniciar a reproducao de \"{channel.Name}\". URL: {url}";
                return;
            }

            NowPlaying = channel;
            SelectedChannel = channel;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao preparar a reproducao de \"{channel.Name}\": {ex.Message}";
        }
    }

    private void OnEncounteredError(object? sender, EventArgs e)
    {
        RunOnUiThread(() =>
        {
            ErrorMessage = _lastAttemptedUrl is null
                ? "Erro ao reproduzir o canal."
                : $"Erro ao reproduzir o canal. URL tentada: {_lastAttemptedUrl}";
            NowPlaying = null;
        });
    }

    private void OnPlaying(object? sender, EventArgs e)
    {
        RunOnUiThread(() => ErrorMessage = null);
    }

    private void OnBuffering(object? sender, MediaPlayerBufferingEventArgs e)
    {
        // Sem acao por enquanto; disponivel para exibir "%" de buffer futuramente.
    }

    private static void RunOnUiThread(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            dispatcher.BeginInvoke(action);
        }
    }

    private void Stop()
    {
        _mediaPlayer.Stop();
        NowPlaying = null;
    }

    private void ToggleFavorite(Channel? channel)
    {
        if (channel is null) return;

        channel.IsFavorite = !channel.IsFavorite;

        if (channel.IsFavorite) _favoriteIds.Add(channel.StreamId);
        else _favoriteIds.Remove(channel.StreamId);

        _favoritesStore.Save(_account, _favoriteIds);

        if (SelectedCategory is { IsFavoritesPseudoGroup: true })
            ApplyFilter();
    }

    public void Dispose()
    {
        _mediaPlayer.EncounteredError -= OnEncounteredError;
        _mediaPlayer.Playing -= OnPlaying;
        _mediaPlayer.Buffering -= OnBuffering;
        _mediaPlayer.Stop();
        _mediaPlayer.Dispose();
        _libVlc.Dispose();
        _xtreamService.Dispose();
    }
}
