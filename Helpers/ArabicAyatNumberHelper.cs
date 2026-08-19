using System.Text;


namespace QuranKemenag.Helpers;

/// <summary>
/// Mengubah nomor ayat menjadi format Mushaf.
/// Contoh:
/// 1 -> ۝١
/// 25 -> ۝٢٥
/// 286 -> ۝٢٨٦
/// </summary>
public static class ArabicNumberHelper
{
    private static readonly char[] ArabicDigits =
    {
        '٠',
        '١',
        '٢',
        '٣',
        '٤',
        '٥',
        '٦',
        '٧',
        '٨',
        '٩'
    };

    /// <summary>
    /// Mengubah angka menjadi angka Arab.
    /// </summary>
    public static string ToArabicNumber(int number)
    {
        var text = number.ToString();

        var builder = new StringBuilder();

        foreach (var c in text)
        {
            builder.Append(
                ArabicDigits[c - '0']);
        }

        return builder.ToString();
    }
}