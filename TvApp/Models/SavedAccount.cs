namespace TvApp.Models;

/// <summary>
/// Conta salva em disco (lista de "perfis" que o usuario ja usou).
/// A senha e guardada protegida via DPAPI (ver AccountStore).
/// </summary>
public class SavedAccount
{
    public string ServerUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string ProtectedPassword { get; set; } = string.Empty;
    public DateTime LastUsedUtc { get; set; } = DateTime.UtcNow;
}
