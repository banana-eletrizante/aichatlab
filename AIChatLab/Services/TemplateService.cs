using System.Reflection;
using System.Text.Json;
using AIChatLab.Models;

namespace AIChatLab.Services;

public static class TemplateService
{
    public static Dictionary<string, string> Generate(Experiment exp)
    {
        var map = BuildMap(exp);
        return new Dictionary<string, string>
        {
            ["index.html"] = Fill(Read("index.html"), map),
            ["styles.css"] = Fill(Read("styles.css"), map),
            ["app.js"] = Fill(Read("app.js"), map),
            ["api/chat.js"] = Fill(Read("chat.js"), map),
            ["vercel.json"] = Fill(Read("vercel.json"), map),
            ["README.md"] = Fill(Read("README.md"), map),
        };
    }

    static string Read(string logical)
    {
        var asm = Assembly.GetExecutingAssembly();
        var name = asm.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(logical, StringComparison.OrdinalIgnoreCase));
        if (name is null)
            throw new InvalidOperationException("template missing: " + logical + " / " + string.Join(",", asm.GetManifestResourceNames()));
        using var s = asm.GetManifestResourceStream(name)!;
        using var r = new StreamReader(s);
        return r.ReadToEnd();
    }

    static string Fill(string template, Dictionary<string, string> map)
    {
        var outp = template;
        foreach (var (k, v) in map)
            outp = outp.Replace("{{" + k + "}}", v, StringComparison.Ordinal);
        return outp;
    }

    static Dictionary<string, string> BuildMap(Experiment exp)
    {
        var branding = exp.ShowBranding
            ? "<footer class=\"branding\">feito com AI Chat Lab · André Rösler</footer>"
            : "";
        return new()
        {
            ["NAME"] = exp.Name.Trim(),
            ["SLUG"] = exp.Slug.Trim(),
            ["TAGLINE"] = exp.Tagline.Trim(),
            ["PERSONA_JS"] = JsonSerializer.Serialize(exp.Persona.Trim()),
            ["WELCOME_JS"] = JsonSerializer.Serialize(exp.WelcomeMessage.Trim()),
            ["WELCOME"] = exp.WelcomeMessage.Trim(),
            ["BG"] = exp.Colors.Bg,
            ["SURFACE"] = exp.Colors.Surface,
            ["TEXT"] = exp.Colors.Text,
            ["ACCENT"] = exp.Colors.Accent,
            ["ACCENT_SOFT"] = exp.Colors.AccentSoft,
            ["DEFAULT_MODEL"] = exp.DefaultModel,
            ["DEFAULT_MODEL_JS"] = JsonSerializer.Serialize(exp.DefaultModel),
            ["MODELS_JSON"] = JsonSerializer.Serialize(exp.AllowedModels),
            ["TEMPERATURE"] = exp.Temperature.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["MAX_TOKENS"] = exp.MaxTokens.ToString(),
            ["LANG"] = exp.Language,
            ["DIR"] = "ltr",
            ["BRANDING_HTML"] = branding,
            ["YEAR"] = DateTime.UtcNow.Year.ToString(),
            ["GRID_OPACITY"] = exp.ShowLabGrid ? "1" : "0",
            ["UI_SETTINGS"] = "Configurações",
            ["UI_MESSAGE"] = "Mensagem",
            ["UI_PLACEHOLDER"] = "Escreva e pressione Enter",
            ["UI_SEND"] = "Enviar",
            ["UI_STOP"] = "Parar",
            ["UI_KEY_TITLE"] = "Chave OpenRouter",
            ["UI_KEY_BODY"] = "Este chat usa a sua chave. Ela fica só neste navegador.",
            ["UI_KEY_HINT"] = "Nada é enviado ao autor do site.",
            ["UI_SAVE_KEY"] = "Salvar chave",
            ["UI_MODEL"] = "Modelo",
            ["UI_CUSTOM_MODEL"] = "Modelo custom",
            ["UI_KEY"] = "Chave OpenRouter",
            ["UI_CLEAR_KEY"] = "Apagar chave",
            ["UI_CLEAR_CHAT"] = "Limpar conversa",
            ["UI_CLOSE"] = "Fechar",
            ["UI_JSON"] = JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["copy"] = "Copiar",
                ["thinking"] = "Pensando…",
                ["previewEcho"] = "[preview] sem rede — publique para conversar de verdade",
                ["badKey"] = "A chave deve começar com sk-or-",
                ["keyCleared"] = "Chave apagada neste navegador.",
                ["err401"] = "Chave inválida.",
                ["err429"] = "Limite da OpenRouter atingido.",
                ["err500"] = "A function falhou.",
                ["errNet"] = "Falha de rede.",
                ["stopped"] = "(geração interrompida)",
            }),
        };
    }
}
