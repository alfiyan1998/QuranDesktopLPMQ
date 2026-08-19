using Microsoft.UI.Xaml.Media;

namespace QuranKemenag.Services.Font;

/// <summary>
/// Menyediakan FontFamily yang digunakan oleh aplikasi.
///
/// Font dibedakan menjadi dua kelompok:
///
/// 1. Unicode Font
///    Digunakan untuk mode Computer Font.
///
/// 2. Symbol Font
///    Digunakan untuk mode Handwritten Manuscript.
/// </summary>
public interface IFontManager
{
    /// <summary>
    /// Mengembalikan FontFamily untuk tampilan Unicode.
    /// </summary>
    FontFamily GetUnicodeFont();

    /// <summary>
    /// Mengembalikan FontFamily sesuai kode font Mushaf.
    /// </summary>
    /// <param name="kodeFont">
    /// Nilai kode_font yang berasal dari database simbol.
    /// </param>
    FontFamily GetSymbolFont(int kodeFont);


    /// <summary>
    /// Mengembalikan FontFamily untuk Bahasa Isyarat.
    /// </summary>
    FontFamily GetIsyaratFont();
}