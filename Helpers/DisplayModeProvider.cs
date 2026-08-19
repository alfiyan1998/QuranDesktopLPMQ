using QuranKemenag.Models;

namespace QuranKemenag.Helpers;

/// <summary>
/// Menyediakan daftar mode tampilan Al-Qur'an.
/// </summary>
public static class DisplayModeProvider
{
    /// <summary>
    /// Menghasilkan seluruh mode tampilan yang tersedia.
    /// </summary>
    public static IReadOnlyList<DisplayModeItem> GetAll()
    {
        return new List<DisplayModeItem>
        {
            new DisplayModeItem(
                DisplayMode.ComputerFont,
                "Computer Font",
                "💻"),

            new DisplayModeItem(
                DisplayMode.HandwrittenManuscript,
                "Handwritten Manuscript",
                "🕌")
        };
    }
}