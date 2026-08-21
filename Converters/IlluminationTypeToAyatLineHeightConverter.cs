using Microsoft.UI.Xaml.Data;
using System;

namespace QuranKemenag.Converters
{
    /// <summary>
    /// LineHeight (satuan kanvas) untuk teks Ayat.
    ///
    /// "AlFatiha": kotak utama tinggi 889 (1147-258) dibagi 15
    /// baris standar Mushaf = ~59 per baris. Dipasangkan dengan
    /// LineStackingStrategy="BlockLineHeight" pada RichTextBlock
    /// supaya setiap baris teks berbaris tepat pada garis
    /// (lihat grid PART_RuledLines) — tidak ada sisa ruang kosong
    /// di bawah ayat karena baris kosong tetap "diisi" garis.
    ///
    /// Selain itu: 0 (otomatis, mengikuti tinggi font alami).
    /// </summary>
    public sealed class IlluminationTypeToAyatLineHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value as string) == "AlFatiha" ? 59d : 0d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
