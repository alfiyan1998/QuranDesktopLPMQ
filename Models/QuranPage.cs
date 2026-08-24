using QuranKemenag.Helpers;
using System;
using System.Collections.Generic;

namespace QuranKemenag.Models
{
    /// <summary>
    /// Merepresentasikan satu halaman mushaf.
    /// Satu halaman dapat berisi beberapa ayat.
    /// </summary>
    public sealed class QuranPage
    {
        /// <summary>
        /// Nomor halaman mushaf.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Juz yang berkaitan dengan halaman ini.
        /// </summary>
        public int? Juz { get; set; }

        /// <summary>
        /// Kumpulan ayat yang terdapat pada halaman ini.
        /// </summary>
        public IReadOnlyList<Ayat> Ayats { get; set; }
            = new List<Ayat>();

        /// <summary>
        /// Susunan baris mushaf pada halaman.
        /// Ini akan digunakan untuk mengatur posisi ayat
        /// agar mengikuti layout halaman mushaf.
        /// </summary>
        public IReadOnlyList<PageLine> Lines { get; set; }
            = new List<PageLine>();

        /// <summary>
        /// Nomor juz dalam format Arab.
        /// </summary>
        public string JuzDisplayText =>
            Juz.HasValue
                ? $"الجزء {ArabicNumberHelper.ToArabicNumber(Juz.Value)}"
                : string.Empty;

        /// <summary>
        /// Nomor surah yang menjadi awal konteks halaman.
        /// </summary>
        public int? SurahNumber { get; set; }

        /// <summary>
        /// Nama Latin surah.
        /// </summary>
        public string SurahLatinName { get; set; } =
            string.Empty;

        /// <summary>
        /// Nama Arab surah.
        /// </summary>
        public string SurahArabicName { get; set; } =
            string.Empty;

        /// <summary>
        /// Apakah halaman dimulai dari ayat pertama suatu surah.
        /// </summary>
        public bool IsStartOfSurah { get; set; }

        /// <summary>
        /// Apakah halaman merupakan awal juz.
        /// </summary>
        public bool IsStartOfJuz { get; set; }

        /// <summary>
        /// Teks Arab footer.
        /// </summary>
        public string FooterArabicText { get; set; } =
            string.Empty;

        /// <summary>
        /// Template halaman mushaf.
        /// </summary>
        public QuranPageTemplate Template { get; set; }
            = QuranPageTemplate.Standard;

        /// <summary>
        /// Informasi header halaman.
        /// </summary>
        public QuranPageHeader Header { get; set; }
            = new QuranPageHeader();

        /// <summary>
        /// Informasi footer halaman.
        /// </summary>
        public QuranPageFooter Footer { get; set; }
            = new QuranPageFooter();

        /// <summary>
        /// Informasi awal surah yang terdeteksi pada halaman.
        /// </summary>
        public IReadOnlyList<SurahStartInfo> SurahStarts { get; set; }
            = new List<SurahStartInfo>();

        /// <summary>
        /// Section surah yang terdapat pada halaman.
        /// Digunakan untuk Bismillah, nama surah,
        /// dan elemen dekorasi.
        /// </summary>
        public IReadOnlyList<SurahSection> SurahSections { get; set; }
            = new List<SurahSection>();
    }
}