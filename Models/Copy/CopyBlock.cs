namespace QuranKemenag.Models.Copy;

public enum CopyBlockType
{
    Arabic,
    Translation,
    Transliteration,
    SignLanguage,
    Footnote,
    SurahInfo,
    Separator
}

public sealed class CopyBlock
{
    public string Text { get; set; } = string.Empty;

    public CopyBlockType Type { get; set; }

    /// <summary>
    /// Font yang digunakan untuk block ini.
    /// Hanya relevan untuk target yang mendukung formatting,
    /// seperti Word dan PowerPoint.
    /// </summary>
    public string? FontFamily { get; set; }

    public double FontSize { get; set; }

    public bool IsRightToLeft { get; set; }
}