using System.Security.Cryptography;
using System.Text;

namespace AIChatLab.Services;

public static class SecretStore
{
    static string PathFile =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AIChatLab", "token.bin");

    public static void SetToken(string raw)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(PathFile)!);
        var bytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(raw), null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(PathFile, bytes);
    }

    public static string? GetToken()
    {
        if (!File.Exists(PathFile)) return null;
        var bytes = ProtectedData.Unprotect(File.ReadAllBytes(PathFile), null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(bytes);
    }

    public static void Clear()
    {
        if (File.Exists(PathFile)) File.Delete(PathFile);
    }
}
