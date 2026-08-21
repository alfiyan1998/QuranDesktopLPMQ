using Microsoft.UI.Xaml.Data;
using System;

namespace QuranKemenag.Converters
{
    /// <summary>
    /// FontSize (dalam satuan kanvas 618x837) untuk teks Ayat,
    /// tergantung IlluminationType. Medalion lingkaran pada
    /// Hafs_Al-Fatiha.png jauh lebih sempit (kotak ±203x203)
    /// dibanding kotak Hafs-Frame.png (±453x692), sehingga
    /// perlu ukuran font lebih kecil supaya tidak overflow.
    /// </summary>
    public sealed class IlluminationTypeToAyatFontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value as string == "AlFatiha")
                return 32d;

            return 26d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
