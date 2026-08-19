using Microsoft.UI.Xaml.Media;
using System.Diagnostics;

namespace QuranKemenag.Services.Font;

/// <summary>
/// Mengelola seluruh FontFamily yang digunakan aplikasi.
///
/// Dibedakan menjadi:
/// 1. Unicode Font
/// 2. Symbol Font
/// </summary>
public sealed class FontManager : IFontManager
{
    /// <summary>
    /// Font Unicode default aplikasi.
    /// </summary>
    public FontFamily GetUnicodeFont()
    {
        const string fontName = "LPMQ IsepMisbah";

        string uri =
            $"ms-appx:///Assets/Fonts/{fontName}.ttf#LPMQ Isep Misbah";

        //Debug.WriteLine("--------------------------------");
        //Debug.WriteLine("Font Manager");
        //Debug.WriteLine("--------------------------------");
        //Debug.WriteLine("Unicode Font");
        //Debug.WriteLine(uri);
        //Debug.WriteLine("--------------------------------");

        return new FontFamily(uri);
    }

    /// <summary>
    /// Mengambil font symbol berdasarkan kode_font.
    /// </summary>
    public FontFamily GetSymbolFont(int kodeFont)
    {
        if (kodeFont <= 0)
        {
            return GetUnicodeFont();
        }

        string fontName =
            $"Lpmq_Hafs_{kodeFont:00}";

        string uri =
            $"ms-appx:///Assets/Fonts/{fontName}-Regular.ttf#{fontName}";

        //Debug.WriteLine("--------------------------------");
        //Debug.WriteLine("Font Manager");
        //Debug.WriteLine("--------------------------------");
        //Debug.WriteLine("Symbol Font");
        //Debug.WriteLine($"Kode Font : {kodeFont}");
        //Debug.WriteLine($"URI       : {uri}");
        //Debug.WriteLine("--------------------------------");

        return new FontFamily(uri);
    }

    /// <summary>
    /// Mengambil font untuk Bahasa Isyarat.
    /// </summary>
    public FontFamily GetIsyaratFont()
    {
        const string fontName = "LPMQ-ISYARAT";

        string uri =
            $"ms-appx:///Assets/Fonts/{fontName}.ttf#LPMQ MSI ISYARAT";

        //Debug.WriteLine("--------------------------------");
        //Debug.WriteLine("Font Manager");
        //Debug.WriteLine("--------------------------------");
        //Debug.WriteLine("Isyarat Font");
        //Debug.WriteLine($"URI : {uri}");
        //Debug.WriteLine("--------------------------------");

        return new FontFamily(uri);
    }
}