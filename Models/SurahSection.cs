namespace QuranKemenag.Models
{
    /// <summary>
    /// Section informasi untuk sebuah permulaan/pergantian surah pada halaman.
    /// Digunakan untuk menentukan apakah Bismillah/iluminasi/surat harus ditampilkan.
    /// </summary>
    public sealed class SurahSection
    {
        /// <summary>
        /// Nomor surah.
        /// </summary>
        public int SurahNumber { get; set; }

        /// <summary>
        /// Nomor ayat tempat section dimulai.
        /// </summary>
        public int StartAyatNumber { get; set; }

        /// <summary>
        /// Posisi pada halaman: Start (awal halaman) atau Middle (tengah halaman).
        /// </summary>
        public SurahStartPosition Position { get; set; }

        /// <summary>
        /// Menandakan apakah halaman ini adalah halaman pertama surah.
        /// (diperkirakan dari posisi Start + ayat pertama)
        /// </summary>
        public bool IsFirstPageOfSurah { get; set; }

        /// <summary>
        /// Apakah Bismillah perlu ditampilkan untuk section ini.
        /// Beberapa surah (mis. At-Taubah) tidak menampilkan Bismillah.
        /// </summary>
        public bool ShowBismillah { get; set; }

        /// <summary>
        /// Apakah ini Surah Al-Fatihah.
        /// </summary>
        public bool IsAlFatiha { get; set; }

        /// <summary>
        /// Apakah ini bagian awal Al-Baqarah ayat 1 (Alif Lam Mim).
        /// </summary>
        public bool IsAlifLamMim { get; set; }
    }
}
