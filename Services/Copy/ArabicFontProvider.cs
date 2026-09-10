using QuranKemenag.Models.Copy;

namespace QuranKemenag.Services.Copy;

public static class ArabicFontProvider
{
    public const string ComputerFontFamily = "ISEP Misbah";

    // Sementara kita isi nama sebenarnya setelah
    // kita memastikan Font Family font Handwritten.
    public const string HandwrittenFontFamily = "HANDWRITTEN_FONT_NAME";

    public static string GetFontFamily(
        ArabicCopyFont font)
    {
        return font switch
        {
            ArabicCopyFont.ComputerFont =>
                ComputerFontFamily,

            ArabicCopyFont.Handwritten =>
                HandwrittenFontFamily,

            _ =>
                ComputerFontFamily
        };
    }
}