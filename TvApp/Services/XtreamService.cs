using System.Net.Http;
using System.Text.Json;
using TvApp.Models;

namespace TvApp.Services;

/// <summary>
/// Cliente HTTP para a API Xtream Codes (o mesmo protocolo usado por
/// players como XCIPTV Player, TiviMate e IPTV Smarters).
///
/// Endpoints usados:
///   {server}/player_api.php?username=..&password=..
///   {server}/player_api.php?username=..&password=..&action=get_live_categories
///   {server}/player_api.php?username=..&password=..&action=get_live_streams[&category_id=..]
///   {server}/live/{username}/{password}/{stream_id}.{ext}   -> URL de reproducao
/// </summary>
public class XtreamService : IDisposable
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public XtreamService()
    {
        _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(20)
        };
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("TvApp/1.0 (WPF; LibVLCSharp)");
    }

    private static string BuildApiUrl(XtreamAccount account, string? action = null, string? extraQuery = null)
    {
        var url = $"{account.NormalizedServerUrl}/player_api.php?username={Uri.EscapeDataString(account.Username)}&password={Uri.EscapeDataString(account.Password)}";
        if (!string.IsNullOrEmpty(action)) url += $"&action={action}";
        if (!string.IsNullOrEmpty(extraQuery)) url += extraQuery;
        return url;
    }

    /// <summary>Faz login e retorna as informacoes de usuario/servidor. Lanca XtreamException se falhar.</summary>
    public async Task<LoginResult> AuthenticateAsync(XtreamAccount account, CancellationToken ct = default)
    {
        string json;
        try
        {
            json = await _http.GetStringAsync(BuildApiUrl(account), ct);
        }
        catch (HttpRequestException ex)
        {
            throw new XtreamException("Nao foi possivel conectar ao servidor. Verifique a URL e sua internet.", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new XtreamException("O servidor demorou demais para responder (timeout).", ex);
        }

        LoginResult? result;
        try
        {
            result = JsonSerializer.Deserialize<LoginResult>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new XtreamException("O servidor respondeu em um formato inesperado. Confirme se a URL esta correta.", ex);
        }

        if (result?.UserInfo is null)
            throw new XtreamException("Usuario ou senha invalidos, ou servidor incompativel.");

        if (!string.Equals(result.UserInfo.Auth.ToString(), "1", StringComparison.Ordinal))
            throw new XtreamException("Falha na autenticacao. Verifique usuario e senha.");

        if (string.Equals(result.UserInfo.Status, "Expired", StringComparison.OrdinalIgnoreCase))
            throw new XtreamException("Esta assinatura esta expirada.");

        return result;
    }

    public async Task<List<ChannelCategory>> GetLiveCategoriesAsync(XtreamAccount account, CancellationToken ct = default)
    {
        var json = await _http.GetStringAsync(BuildApiUrl(account, "get_live_categories"), ct);
        return JsonSerializer.Deserialize<List<ChannelCategory>>(json, JsonOptions) ?? new List<ChannelCategory>();
    }

    public async Task<List<Channel>> GetLiveStreamsAsync(XtreamAccount account, string? categoryId = null, CancellationToken ct = default)
    {
        var extra = categoryId is null ? null : $"&category_id={Uri.EscapeDataString(categoryId)}";
        var json = await _http.GetStringAsync(BuildApiUrl(account, "get_live_streams", extra), ct);
        var channels = JsonSerializer.Deserialize<List<Channel>>(json, JsonOptions) ?? new List<Channel>();
        foreach (var c in channels) c.CategoryId ??= categoryId;
        return channels;
    }

    /// <summary>Monta a URL de reproducao do canal ao vivo.</summary>
    public static string BuildLiveStreamUrl(XtreamAccount account, Channel channel)
    {
        var ext = string.IsNullOrWhiteSpace(channel.ContainerExtension) ? "ts" : channel.ContainerExtension;
        return $"{account.NormalizedServerUrl}/live/{Uri.EscapeDataString(account.Username)}/{Uri.EscapeDataString(account.Password)}/{channel.StreamId}.{ext}";
    }

    public void Dispose() => _http.Dispose();
}
