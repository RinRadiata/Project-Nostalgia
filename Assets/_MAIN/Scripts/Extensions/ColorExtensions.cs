using Unity.VisualScripting;
using UnityEngine;

public static class ColorExtensions
{
    public static Color SetAlpha(this Color original, float alpha)
    {
        return new Color(original.r, original.g, original.b, alpha);
    }

    public static Color GetColorFromName(this Color original, string colorName)
    {
        switch (colorName.ToLower())
        {
            case "red":
                return Color.red;
            case "green":
                return Color.green;
            case "blue":
                return Color.blue;
            case "yellow":
                return Color.yellow;
            case "black":
                return Color.black;
            case "white":
                return Color.white;
            case "gray":
            case "grey":
                return Color.gray;
            case "cyan":
                return Color.cyan;
            case "magenta":
                return Color.magenta;
            case "orange":
                return new Color(1.0f, 0.647f, 0.0f); // RGB for orange
            default:
                Debug.LogWarning($"Color '{colorName}' not recognized.");
                return Color.clear;
        }
    }
}
