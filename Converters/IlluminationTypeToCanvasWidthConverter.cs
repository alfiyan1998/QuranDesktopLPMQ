using Microsoft.UI.Xaml.Data;
using System;

namespace QuranKemenag.Converters
{
    /// <summary>
    /// Lebar kanvas asli (piksel) untuk asset iluminasi aktif.
    /// Diperlukan karena asset Hafs_Al-Fatiha.png (versi baru,
    /// kotak — bukan medalion lingkaran) berukuran 1122x1402,
    /// berbeda dari Hafs-Frame.png yang 618x837. Grid pembungkus
    /// di dalam Viewbox harus memakai ukuran asli asset yang
    /// sedang aktif supaya margin (dalam satuan piksel asli) tetap
    /// presisi terhadap asset tsb.
    /// </summary>
    public sealed class IlluminationTypeToCanvasWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value as string) == "AlFatiha" ? 1122d : 618d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
