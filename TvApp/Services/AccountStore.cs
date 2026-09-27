using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TvApp.Models;

namespace TvApp.Services;

/// <summary>
/// Guarda as contas usadas (URL/usuario) e a senha protegida com DPAPI
/// (a senha so pode ser lida de volta na mesma maquina/usuario do Windows).
/// </summary>
public class AccountStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public List<SavedAccount> LoadAll()
    {
        try
        {
            if (!File.Exists(PathHelper.AccountsFile)) return new List<SavedAccount>();
            var json = File.ReadAllText(PathHelper.AccountsFile);
            return JsonSerializer.Deserialize<List<SavedAccount>>(json) ?? new List<SavedAccount>();
        }
        catch
        {
            return new List<SavedAccount>();
        }
    }

    public SavedAccount? LoadLastUsed()
    {
        return LoadAll().OrderByDescending(a => a.LastUsedUtc).FirstOrDefault();
    }

    public void Save(XtreamAccount account)
    {
        var all = LoadAll();
        var existing = all.FirstOrDefault(a =>
            string.Equals(a.ServerUrl, account.ServerUrl, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(a.Username, account.Username, StringComparison.OrdinalIgnoreCase));

        var protectedPassword = Protect(account.Password);

        if (existing is null)
        {
            all.Add(new SavedAccount
            {
                ServerUrl = account.ServerUrl,
                Username = account.Username,
                ProtectedPassword = protectedPassword,
                LastUsedUtc = DateTime.UtcNow
            });
        }
        else
        {
            existing.ProtectedPassword = protectedPassword;
            existing.LastUsedUtc = DateTime.UtcNow;
        }

        File.WriteAllText(PathHelper.AccountsFile, JsonSerializer.Serialize(all, JsonOptions));
    }

    public void Remove(SavedAccount account)
    {
        var all = LoadAll();
        all.RemoveAll(a =>
            string.Equals(a.ServerUrl, account.ServerUrl, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(a.Username, account.Username, StringComparison.OrdinalIgnoreCase));
        File.WriteAllText(PathHelper.AccountsFile, JsonSerializer.Serialize(all, JsonOptions));
    }

    public string Unprotect(string protectedPassword)
    {
        try
        {
            var bytes = Convert.FromBase64String(protectedPassword);
            var plain = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plain);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string Protect(string plainPassword)
    {
        var bytes = Encoding.UTF8.GetBytes(plainPassword);
        var protectedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(protectedBytes);
    }
}
