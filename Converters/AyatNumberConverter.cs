using Microsoft.UI.Xaml.Data;
using System;
using System.Text;

namespace QuranKemenag.Converters
{
    /// <summary>
    /// Convert an integer ayat number to Arabic-Indic digits prefixed with the Arabic end-of-ayah symbol.
    /// Example: 1 -> " \u06DD\u0661" which renders as " ۝١".
    /// </summary>
    public sealed class AyatNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return string.Empty;

            if (!int.TryParse(value.ToString(), out var n))
                return string.Empty;

            var digits = n.ToString();
            var sb = new StringBuilder();
            foreach (var c in digits)
            {
                if (char.IsDigit(c))
                {
                    // Arabic-Indic digits U+0660 .. U+0669
                    sb.Append((char)('\u0660' + (c - '0')));
                }
                else
                {
                    sb.Append(c);
                }
            }

            // Arabic end of ayah symbol U+06DD followed by Arabic-Indic digits. Prefix a regular space so it visually separates from the text.
            return " " + '\u06DD' + sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
