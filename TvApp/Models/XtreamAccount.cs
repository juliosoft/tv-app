namespace TvApp.Models;

/// <summary>
/// Dados de conexao com o servidor IPTV (padrao Xtream Codes).
/// </summary>
public class XtreamAccount
{
    public string ServerUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Chave usada para separar favoritos/cache entre contas diferentes.
    /// </summary>
    public string Key => $"{NormalizedServerUrl}|{Username}".ToLowerInvariant();

    public string NormalizedServerUrl
    {
        get
        {
            var url = ServerUrl.Trim().TrimEnd('/');
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "http://" + url;
            }
            return url;
        }
    }
}
