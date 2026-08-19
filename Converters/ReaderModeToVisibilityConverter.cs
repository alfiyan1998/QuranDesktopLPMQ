using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using QuranKemenag.Models;
using System;

namespace QuranKemenag.Converters
{
    public sealed class ReaderModeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            if (!Enum.TryParse(typeof(ReaderMode), value.ToString(), out var current))
                return Visibility.Collapsed;

            var param = parameter.ToString();
            if (string.Equals(param, "Unicode", StringComparison.OrdinalIgnoreCase))
            {
                return (ReaderMode)current == ReaderMode.Unicode ? Visibility.Visible : Visibility.Collapsed;
            }

            if (string.Equals(param, "Symbol", StringComparison.OrdinalIgnoreCase))
            {
                return (ReaderMode)current == ReaderMode.Symbol ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
