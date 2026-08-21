using Microsoft.UI.Xaml.Data;
using System;

namespace QuranKemenag.Converters
{
    /// <summary>
    /// Tinggi kanvas asli (piksel) untuk asset iluminasi aktif.
    /// Pasangan dari IlluminationTypeToCanvasWidthConverter — lihat
    /// catatan di sana.
    /// </summary>
    public sealed class IlluminationTypeToCanvasHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value as string) == "AlFatiha" ? 1402d : 837d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
