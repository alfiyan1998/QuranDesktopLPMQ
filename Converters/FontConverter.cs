using Microsoft.UI.Xaml.Data;
using QuranKemenag.Services.Font;
using System;

namespace QuranKemenag.Converters;

/// <summary>
/// Mengubah nilai kode_font menjadi FontFamily.
/// </summary>
public sealed class FontConverter : IValueConverter
{
    private readonly IFontManager _fontManager;

    public FontConverter()
    {
        _fontManager =
            (IFontManager)App.Services.GetService(typeof(IFontManager))!;
    }

    public object Convert(
        object value,
        Type targetType,
        object parameter,
        string language)
    {
        if (value is not int kodeFont)
        {
            kodeFont = 0;
        }

        return _fontManager.GetSymbolFont(kodeFont);
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        string language)
    {
        throw new NotImplementedException();
    }
}