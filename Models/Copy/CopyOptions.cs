namespace QuranKemenag.Models.Copy;

public sealed class CopyOptions
{
    // ================================
    // KONTEN
    // ================================

    public bool IncludeArabic { get; set; } = true;

    public bool IncludeTranslation { get; set; } = true;

    public bool IncludeTransliteration { get; set; }

    public bool IncludeSignLanguage { get; set; }


    // ================================
    // FONT ARAB
    // ================================

    public ArabicCopyFont ArabicFont { get; set; }
        = ArabicCopyFont.ComputerFont;


    // ================================
    // OPSI PENYALINAN
    // ================================

    public bool AddParentheses { get; set; }

    public bool AlternateContent { get; set; }

    public bool AddAyahNumber { get; set; }

    public bool AddSurahAyah { get; set; }


    // ================================
    // TARGET
    // ================================

    public CopyTarget Target { get; set; }
        = CopyTarget.Word;
}