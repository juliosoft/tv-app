using System.IO;

namespace TvApp.Services;

internal static class PathHelper
{
    public static string AppDataFolder
    {
        get
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TvApp");
            Directory.CreateDirectory(folder);
            return folder;
        }
    }

    public static string FavoritesFolder
    {
        get
        {
            var folder = Path.Combine(AppDataFolder, "favorites");
            Directory.CreateDirectory(folder);
            return folder;
        }
    }

    public static string AccountsFile => Path.Combine(AppDataFolder, "accounts.json");
}
