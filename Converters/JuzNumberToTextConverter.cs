using Microsoft.UI.Xaml.Data;
using System;

namespace QuranKemenag.Converters
{
    /// <summary>
    /// Mengubah nomor Juz (int?) menjadi teks tampilan "JUZ X"
    /// (huruf besar, sesuai badge referensi) untuk badge Mushaf.
    /// Dipisah dari ViewModel supaya ViewModel tetap hanya
    /// menyimpan data mentah (nomor Juz), sementara format
    /// tampilan menjadi tanggung jawab View (lihat STEP 25.12 §17).
    /// </summary>
    public sealed class JuzNumberToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int juz && juz > 0)
                return $"JUZ {juz}";

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
