using System.Text.Json.Serialization;

namespace TvApp.Models;

/// <summary>
/// Grupo/categoria de canais, exatamente como o servidor organiza (get_live_categories).
/// </summary>
public class ChannelCategory
{
    [JsonPropertyName("category_id")]
    public string CategoryId { get; set; } = string.Empty;

    [JsonPropertyName("category_name")]
    public string CategoryName { get; set; } = string.Empty;

    [JsonPropertyName("parent_id")]
    public int ParentId { get; set; }

    /// <summary>Categoria virtual usada para exibir os favoritos (nao vem do servidor).</summary>
    [JsonIgnore]
    public bool IsFavoritesPseudoGroup { get; set; }

    public override string ToString() => CategoryName;
}
