using System.Text.Json.Serialization;

namespace TvApp.Models;

public class LoginResult
{
    [JsonPropertyName("user_info")]
    public UserInfo? UserInfo { get; set; }

    [JsonPropertyName("server_info")]
    public ServerInfo? ServerInfo { get; set; }
}

public class UserInfo
{
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("auth")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Auth { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("exp_date")]
    public string? ExpDate { get; set; }

    [JsonPropertyName("max_connections")]
    public string? MaxConnections { get; set; }
}

public class ServerInfo
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("port")]
    public string? Port { get; set; }

    [JsonPropertyName("https_port")]
    public string? HttpsPort { get; set; }

    [JsonPropertyName("server_protocol")]
    public string? ServerProtocol { get; set; }
}
