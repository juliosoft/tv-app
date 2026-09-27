using System.Windows;
using System.Windows.Input;
using TvApp.Models;
using TvApp.Services;
using TvApp.ViewModels;

namespace TvApp.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private bool _isFullScreen;
    private double _savedCategoriesWidth = 230;
    private double _savedChannelsWidth = 340;
    private double _savedTopBarHeight = 52;
    private WindowState _savedWindowState;
    private WindowStyle _savedWindowStyle;
    private ResizeMode _savedResizeMode;

    public MainWindow(XtreamAccount account)
    {
        InitializeComponent();

        var xtreamService = new XtreamService();
        var favoritesStore = new FavoritesStore();
        _viewModel = new MainViewModel(account, xtreamService, favoritesStore);
        _viewModel.RequestLogout += OnRequestLogout;

        DataContext = _viewModel;

        Loaded += async (_, _) => await _viewModel.InitializeAsync();
        Closed += (_, _) => _viewModel.Dispose();
    }

    private void ChannelsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ChannelsListBox.SelectedItem is Channel channel && _viewModel.PlayChannelCommand.CanExecute(channel))
        {
            _viewModel.PlayChannelCommand.Execute(channel);
        }
    }

    private void FullscreenButton_Click(object sender, RoutedEventArgs e) => ToggleFullScreen();

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && _isFullScreen)
        {
            ToggleFullScreen();
        }
        else if (e.Key == Key.F11)
        {
            ToggleFullScreen();
        }
    }

    private void ToggleFullScreen()
    {
        if (!_isFullScreen)
        {
            _savedCategoriesWidth = CategoriesColumn.Width.Value;
            _savedChannelsWidth = ChannelsColumn.Width.Value;
            _savedTopBarHeight = TopBarRow.Height.Value;
            _savedWindowState = WindowState;
            _savedWindowStyle = WindowStyle;
            _savedResizeMode = ResizeMode;

            CategoriesColumn.Width = new GridLength(0);
            ChannelsColumn.Width = new GridLength(0);
            TopBarRow.Height = new GridLength(0);

            // A ordem importa: alternar para Normal antes de trocar o estilo e so
            // depois maximizar evita um bug conhecido do WPF em que a barra de
            // tarefas continua visivel ao maximizar uma janela sem borda (WindowStyle=None).
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Maximized;
        }
        else
        {
            WindowState = WindowState.Normal;
            ResizeMode = _savedResizeMode;
            WindowStyle = _savedWindowStyle;
            WindowState = _savedWindowState;

            CategoriesColumn.Width = new GridLength(_savedCategoriesWidth);
            ChannelsColumn.Width = new GridLength(_savedChannelsWidth);
            TopBarRow.Height = new GridLength(_savedTopBarHeight);
        }

        _isFullScreen = !_isFullScreen;
    }

    private void OnRequestLogout()
    {
        var loginWindow = new LoginWindow();
        Application.Current.MainWindow = loginWindow;
        loginWindow.Show();
        Close();
    }
}
