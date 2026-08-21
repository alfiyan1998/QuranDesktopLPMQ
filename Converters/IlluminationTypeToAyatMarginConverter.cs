using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace QuranKemenag.Converters
{
    /// <summary>
    /// Margin area teks Ayat, tergantung IlluminationType. Nilai
    /// diukur langsung dari piksel asset PNG (bukan perkiraan):
    ///
    /// - "AlFatiha": Hafs_Al-Fatiha.png (versi kotak, 1122x1402).
    ///   Kotak putih utama diukur lewat flood-fill: kiri=157,
    ///   kanan=962, atas=258, bawah=1147.
    ///
    /// - selain itu (Hafs-Frame.png, 618x837): kotak putih polos,
    ///   margin diukur langsung dari tepi area putih di dalam
    ///   bingkai.
    /// </summary>
    public sealed class IlluminationTypeToAyatMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value as string == "AlFatiha")
                return new Thickness(157, 258, 160, 255);

            return new Thickness(82, 106, 83, 39);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
