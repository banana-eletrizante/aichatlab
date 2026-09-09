namespace AIChatLab.Helpers;

public static class ColorMath
{
    public static string Mix(string hex, double amountTowardWhite)
    {
        hex = hex.TrimStart('#');
        if (hex.Length != 6) return "#" + hex;
        int r = Convert.ToInt32(hex[..2], 16);
        int g = Convert.ToInt32(hex[2..4], 16);
        int b = Convert.ToInt32(hex[4..6], 16);
        r = (int)(r + (255 - r) * amountTowardWhite);
        g = (int)(g + (255 - g) * amountTowardWhite);
        b = (int)(b + (255 - b) * amountTowardWhite);
        return $"#" + r.ToString("X2") + g.ToString("X2") + b.ToString("X2");
    }
}
