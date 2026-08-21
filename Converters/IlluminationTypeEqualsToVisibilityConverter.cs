using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace QuranKemenag.Converters
{
    /// <summary>
    /// Menampilkan elemen hanya jika IlluminationType sama dengan
    /// ConverterParameter (mis. "AlFatiha"). Dipakai untuk kartus
    /// nama Surah yang letaknya spesifik pada asset tertentu
    /// (Hafs_Al-Fatiha.png), sehingga tidak ikut tampil pada
    /// halaman yang memakai Hafs-Frame.png.
    /// </summary>
    public sealed class IlluminationTypeEqualsToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var expected = parameter as string;

            return (value as string) == expected
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
