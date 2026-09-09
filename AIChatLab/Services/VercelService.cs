using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AIChatLab.Models;

namespace AIChatLab.Services;

public sealed class VercelService
{
    readonly HttpClient _http = new();
    string? _token;

    public void SetToken(string raw) => _token = raw;
    public string? GetToken() => _token;

    async Task<HttpResponseMessage> Call(string path, HttpMethod method, object? body = null)
    {
        var req = new HttpRequestMessage(method, "https://api.vercel.com" + path);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        if (body is not null)
            req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        return await _http.SendAsync(req);
    }

    public async Task<(bool ok, string label)> WhoAmI()
    {
        var r = await Call("/v2/user", HttpMethod.Get);
        if ((int)r.StatusCode is 401 or 403) return (false, "token_invalid");
        if (!r.IsSuccessStatusCode) return (false, "http_" + (int)r.StatusCode);
        using var doc = JsonDocument.Parse(await r.Content.ReadAsStringAsync());
        var user = doc.RootElement.GetProperty("user");
        var name = user.TryGetProperty("username", out var u) ? u.GetString() : "";
        return (true, name ?? "");
    }

    public async Task<DeployResult> DeployFolder(string folder, string projectName)
    {
        var files = new List<object>();
        foreach (var path in Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(folder, path).Replace('\\', '/');
            files.Add(new { file = rel, data = File.ReadAllText(path) });
        }
        var r = await Call("/v13/deployments?forceNew=1", HttpMethod.Post, new
        {
            name = projectName,
            project = projectName,
            target = "production",
            files,
        });
        var code = (int)r.StatusCode;
        var text = await r.Content.ReadAsStringAsync();
        if (code is 401 or 403) return new() { Ok = false, Error = "token_invalid" };
        if (code == 429) return new() { Ok = false, Error = "rate_limited" };
        if (!r.IsSuccessStatusCode)
            return new() { Ok = false, Error = text.Contains("conflict", StringComparison.OrdinalIgnoreCase) ? "name_taken" : "http_" + code };

        using var created = JsonDocument.Parse(text);
        var id = created.RootElement.GetProperty("id").GetString();
        var urlHint = created.RootElement.TryGetProperty("url", out var u) ? u.GetString() : null;
        var deadline = DateTime.UtcNow.AddSeconds(90);
        while (DateTime.UtcNow < deadline)
        {
            var s = await Call("/v13/deployments/" + id, HttpMethod.Get);
            if (!s.IsSuccessStatusCode)
            {
                await Task.Delay(2000);
                continue;
            }
            using var body = JsonDocument.Parse(await s.Content.ReadAsStringAsync());
            var state = body.RootElement.TryGetProperty("readyState", out var rs) ? rs.GetString() : "";
            if (state == "READY")
            {
                var host = body.RootElement.TryGetProperty("url", out var urlEl) ? urlEl.GetString() : urlHint;
                var url = host is null ? "" : host.StartsWith("http", StringComparison.Ordinal) ? host : "https://" + host;
                return new() { Ok = true, Url = url };
            }
            if (state == "ERROR") return new() { Ok = false, Error = "deploy_error" };
            await Task.Delay(2000);
        }
        return new() { Ok = false, Error = "timeout" };
    }
}
