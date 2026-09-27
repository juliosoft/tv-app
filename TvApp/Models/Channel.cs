using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace TvApp.Models;

/// <summary>
/// Canal de TV ao vivo, conforme retornado por get_live_streams.
/// Alguns servidores retornam numeros como string, por isso os
/// campos numericos usam JsonNumberHandling para aceitar os dois formatos.
/// </summary>
public class Channel : INotifyPropertyChanged
{
    [JsonPropertyName("num")]
    public int Num { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("stream_type")]
    public string StreamType { get; set; } = string.Empty;

    [JsonPropertyName("stream_id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int StreamId { get; set; }

    [JsonPropertyName("stream_icon")]
    public string? StreamIcon { get; set; }

    [JsonPropertyName("epg_channel_id")]
    public string? EpgChannelId { get; set; }

    [JsonPropertyName("category_id")]
    public string? CategoryId { get; set; }

    [JsonPropertyName("tv_archive")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int TvArchive { get; set; }

    [JsonPropertyName("custom_sid")]
    public string? CustomSid { get; set; }

    [JsonPropertyName("direct_source")]
    public string? DirectSource { get; set; }

    [JsonIgnore]
    public string ContainerExtension { get; set; } = "ts";

    private bool _isFavorite;

    [JsonIgnore]
    public bool IsFavorite
    {
        get => _isFavorite;
        set
        {
            if (_isFavorite == value) return;
            _isFavorite = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
