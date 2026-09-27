using System.Windows.Input;
using TvApp.Models;
using TvApp.Services;

namespace TvApp.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly XtreamService _xtreamService;
    private readonly AccountStore _accountStore;

    private string _serverUrl = string.Empty;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private bool _rememberMe = true;
    private bool _isBusy;
    private string? _errorMessage;

    public LoginViewModel(XtreamService xtreamService, AccountStore accountStore)
    {
        _xtreamService = xtreamService;
        _accountStore = accountStore;

        LoginCommand = new AsyncRelayCommand(ExecuteLoginAsync, () => !IsBusy && CanTryLogin());

        LoadLastUsedAccount();
    }

    public string ServerUrl
    {
        get => _serverUrl;
        set { if (SetField(ref _serverUrl, value)) OnPropertyChanged(nameof(CanTryLogin)); }
    }

    public string Username
    {
        get => _username;
        set { if (SetField(ref _username, value)) OnPropertyChanged(nameof(CanTryLogin)); }
    }

    public string Password
    {
        get => _password;
        set { if (SetField(ref _password, value)) OnPropertyChanged(nameof(CanTryLogin)); }
    }

    public bool RememberMe
    {
        get => _rememberMe;
        set => SetField(ref _rememberMe, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetField(ref _isBusy, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetField(ref _errorMessage, value);
    }

    public bool CanTryLogin() =>
        !string.IsNullOrWhiteSpace(ServerUrl) &&
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password);

    public ICommand LoginCommand { get; }

    /// <summary>Disparado quando o login foi bem-sucedido, levando a conta autenticada.</summary>
    public event Action<XtreamAccount>? LoginSucceeded;

    private void LoadLastUsedAccount()
    {
        var last = _accountStore.LoadLastUsed();
        if (last is null) return;

        ServerUrl = last.ServerUrl;
        Username = last.Username;
        Password = _accountStore.Unprotect(last.ProtectedPassword);
    }

    private async Task ExecuteLoginAsync()
    {
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var account = new XtreamAccount
            {
                ServerUrl = ServerUrl.Trim(),
                Username = Username.Trim(),
                Password = Password
            };

            await _xtreamService.AuthenticateAsync(account);

            if (RememberMe)
            {
                _accountStore.Save(account);
            }

            LoginSucceeded?.Invoke(account);
        }
        catch (XtreamException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro inesperado: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
