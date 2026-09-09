using AIChatLab.Models;

namespace AIChatLab.Helpers;

public static class Validator
{
    public static string? ValidateForGenerate(Experiment exp)
    {
        var name = exp.Name.Trim();
        if (name.Length is < 2 or > 48) return "Nome deve ter 2–48 caracteres.";
        if (!Slug.IsValid(exp.Slug.Trim())) return "Slug deve ter 3–48 caracteres: a-z, 0-9, hífen.";
        if (exp.Persona.Trim().Length < 20) return "Persona deve ter pelo menos 20 caracteres.";
        if (exp.AllowedModels.Count == 0) return "Escolha pelo menos um modelo.";
        if (!exp.AllowedModels.Contains(exp.DefaultModel)) return "O modelo padrão precisa estar na lista liberada.";
        return null;
    }
}
