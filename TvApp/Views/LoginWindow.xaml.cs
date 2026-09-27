using System.Windows;
using TvApp.Models;
using TvApp.Services;
using TvApp.ViewModels;

namespace TvApp.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;

    public LoginWindow()
    {
        InitializeComponent();

        var xtreamService = new XtreamService();
        var accountStore = new AccountStore();
        _viewModel = new LoginViewModel(xtreamService, accountStore);
        _viewModel.LoginSucceeded += OnLoginSucceeded;

        DataContext = _viewModel;

        PasswordBox.Password = _viewModel.Password;
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _viewModel.Password = PasswordBox.Password;
    }

    private void OnLoginSucceeded(XtreamAccount account)
    {
        var mainWindow = new MainWindow(account);
        Application.Current.MainWindow = mainWindow;
        mainWindow.Show();
        Close();
    }
}
