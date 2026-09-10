using Microsoft.UI.Xaml.Data;
using QuranKemenag.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuranKemenag.Converters;

public sealed class PageLineToTextConverter : IValueConverter
{
    // RLE (Right-to-Left Embedding) ... PDF (Pop Directional Formatting).
    // Memaksa SELURUH isi baris diperlakukan sebagai satu blok RTL,
    // supaya karakter yang secara default dianggap "netral/LTR" oleh
    // algoritma bidi Unicode (misalnya kode PUA font Handwritten)
    // tidak membuat urutan kata terbalik.
    private const char RightToLeftEmbedding = '\u202B';
    private const char PopDirectionalFormatting = '\u202C';

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not IEnumerable<PageWord> words)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.Append(RightToLeftEmbedding);

        foreach (var word in words)
        {
            var text = string.IsNullOrEmpty(word.DisplayText)
                ? word.WordText
                : word.DisplayText;

            if (sb.Length > 1)
            {
                sb.Append(' ');
            }

            sb.Append(text);
        }

        sb.Append(PopDirectionalFormatting);

        return sb.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}