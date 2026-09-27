using System;
using System.Windows;
using LibVLCSharp.Shared;

namespace TvApp;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Inicializa o LibVLC uma unica vez para todo o app.
        try
        {
            Core.Initialize();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Nao foi possivel inicializar o componente de video (LibVLC).\n" +
                "Verifique se o pacote VideoLAN.LibVLC.Windows foi restaurado corretamente.\n\n" +
                ex.Message,
                "Erro ao iniciar",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        DispatcherUnhandledException += (_, args) =>
        {
            MessageBox.Show(args.Exception.Message, "Erro inesperado", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };
    }
}
