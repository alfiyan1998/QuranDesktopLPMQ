using System.Collections.Generic;

namespace QuranKemenag.Models
{
    /// <summary>
    /// Informasi header untuk satu halaman mushaf.
    /// Menyimpan informasi surat/juz/posisi halaman yang diperlukan
    /// untuk merender header secara dinamis.
    /// </summary>
    public sealed class QuranPageHeader
    {
        /// <summary>
        /// Nomor halaman yang berkaitan dengan header ini.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Nomor juz terkait halaman (jika ada).
        /// </summary>
        public int? Juz { get; set; }

        /// <summary>
        /// Daftar nomor surah yang tampil pada halaman (urut, unik).
        /// Bisa berisi lebih dari satu jika terjadi pergantian surah di halaman.
        /// </summary>
        public IReadOnlyList<int> SurahNumbers { get; set; } = new List<int>();

        /// <summary>
        /// Nomor surah utama (surah pertama yang tampil pada halaman).
        /// </summary>
        public int? PrimarySurahNumber { get; set; }

        /// <summary>
        /// Nama Latin surah utama (jika tersedia).
        /// </summary>
        public string PrimarySurahLatinName { get; set; } = string.Empty;

        /// <summary>
        /// Nama Arab surah utama (jika tersedia).
        /// </summary>
        public string PrimarySurahArabicName { get; set; } = string.Empty;

        /// <summary>
        /// Menandakan apakah halaman merupakan awal sebuah surah.
        /// </summary>
        public bool IsSurahStart { get; set; }
    }
}
