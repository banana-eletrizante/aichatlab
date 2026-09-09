namespace AIChatLab.Services;

public static class FileExportService
{
    public static void WriteFolder(string folder, Dictionary<string, string> files)
    {
        Directory.CreateDirectory(folder);
        foreach (var (path, contents) in files)
        {
            var full = Path.Combine(folder, path.Replace('/', Path.DirectorySeparatorChar));
            var dir = Path.GetDirectoryName(full);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(full, contents);
        }
    }
}
