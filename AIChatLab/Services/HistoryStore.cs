using System.Text.Json;
using AIChatLab.Models;

namespace AIChatLab.Services;

public static class HistoryStore
{
    static string FilePath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AIChatLab", "history.json");

    public static List<Experiment> Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return [];
            return JsonSerializer.Deserialize<List<Experiment>>(File.ReadAllText(FilePath)) ?? [];
        }
        catch { return []; }
    }

    public static void Upsert(Experiment exp)
    {
        var items = Load().Where(x => x.Id != exp.Id).ToList();
        exp.UpdatedAt = DateTime.UtcNow;
        items.Insert(0, exp);
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(items.Take(40)));
    }

    public static void Remove(Guid id)
    {
        var items = Load().Where(x => x.Id != id).ToList();
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(items));
    }
}
