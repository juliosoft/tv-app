using System.IO;
using System.Text.Json;
using TvApp.Models;

namespace TvApp.Services;

/// <summary>
/// Persiste a lista de stream_id favoritados, separada por conta (account.Key),
/// para que cada login tenha seus proprios favoritos.
/// </summary>
public class FavoritesStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private string FileFor(XtreamAccount account)
    {
        var safeName = string.Concat(account.Key.Select(c => char.IsLetterOrDigit(c) ? c : '_'));
        return Path.Combine(PathHelper.FavoritesFolder, $"{safeName}.json");
    }

    public HashSet<int> Load(XtreamAccount account)
    {
        try
        {
            var file = FileFor(account);
            if (!File.Exists(file)) return new HashSet<int>();
            var json = File.ReadAllText(file);
            var ids = JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
            return new HashSet<int>(ids);
        }
        catch
        {
            return new HashSet<int>();
        }
    }

    public void Save(XtreamAccount account, IEnumerable<int> streamIds)
    {
        var file = FileFor(account);
        File.WriteAllText(file, JsonSerializer.Serialize(streamIds.Distinct().ToList(), JsonOptions));
    }
}
