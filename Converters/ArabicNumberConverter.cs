using Microsoft.UI.Xaml.Data;
using System;

namespace QuranKemenag.Converters
{
    public sealed class ArabicNumberConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            string language)
        {
            if (value == null)
                return string.Empty;

            if (!int.TryParse(value.ToString(), out var number))
                return string.Empty;

            var digits = number.ToString();

            return digits
                .Replace('0', '٠')
                .Replace('1', '١')
                .Replace('2', '٢')
                .Replace('3', '٣')
                .Replace('4', '٤')
                .Replace('5', '٥')
                .Replace('6', '٦')
                .Replace('7', '٧')
                .Replace('8', '٨')
                .Replace('9', '٩');
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            string language)
        {
            throw new NotSupportedException();
        }
    }
}