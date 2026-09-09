namespace AIChatLab.Models;

public sealed class ThemeColors
{
    public string Bg { get; set; } = "#06140f";
    public string Surface { get; set; } = "#0a1f18";
    public string Text { get; set; } = "#e8fff5";
    public string Accent { get; set; } = "#22c55e";
    public string AccentSoft { get; set; } = "#4ade80";
    public string Danger { get; set; } = "#f87171";
}

public sealed class Experiment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Tagline { get; set; } = "";
    public string Persona { get; set; } = "";
    public string WelcomeMessage { get; set; } = "";
    public string ThemeId { get; set; } = "lab-green";
    public ThemeColors Colors { get; set; } = new();
    public string AccentName { get; set; } = "lab-green";
    public string DefaultModel { get; set; } = "openai/gpt-4o-mini";
    public List<string> AllowedModels { get; set; } =
    [
        "openai/gpt-4o-mini",
        "openai/gpt-4o",
        "anthropic/claude-3.5-sonnet",
        "google/gemini-2.0-flash-001",
        "meta-llama/llama-3.3-70b-instruct",
        "deepseek/deepseek-chat",
        "mistralai/mistral-nemo",
    ];
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 1024;
    public string Language { get; set; } = "pt";
    public bool ShowBranding { get; set; } = true;
    public bool ShowLabGrid { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? LastDeployUrl { get; set; }
    public string? LastProjectName { get; set; }
}

public sealed class DeployResult
{
    public bool Ok { get; set; }
    public string? Url { get; set; }
    public string? Error { get; set; }
}
