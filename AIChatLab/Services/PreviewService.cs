namespace AIChatLab.Services;

public static class PreviewService
{
    public static string ToSrcDoc(Dictionary<string, string> files)
    {
        var html = files["index.html"];
        var css = files["styles.css"];
        var js = files["app.js"];
        return html
            .Replace("<link rel=\"stylesheet\" href=\"./styles.css\" />", "<style>" + css + "</style>")
            .Replace("<script src=\"./app.js\"></script>",
                "<script>window.__AICHATLAB_PREVIEW__=true;</script><script>" + js + "</script>");
    }
}
