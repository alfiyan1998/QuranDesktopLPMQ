using QuranKemenag.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Nullable karena data halaman belum tersedia
        /// pada database saat ini.
        /// </summary>
        public int? Juz { get; set; }

        /// <summary>
        /// Kumpulan ayat yang terdapat pada halaman ini.
        /// </summary>
        public IReadOnlyList<Ayat> Ayats { get; set; }
            = new List<Ayat>();

        /// <summary>
        /// Nomor juz dalam format Arab
        /// untuk kebutuhan header mushaf.
        ///
        /// Contoh:
        /// الجزء ١
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
        /// Digunakan untuk metadata/debug.
        /// </summary>
        public string SurahLatinName { get; set; } =
            string.Empty;

        /// <summary>
        /// Nama Arab surah.
        /// Digunakan pada header mushaf.
        /// </summary>
        public string SurahArabicName { get; set; } =
            string.Empty;

        /// <summary>
        /// Apakah halaman dimulai dari ayat pertama suatu surah.
        ///
        /// Ini akan sangat penting untuk:
        /// - Bismillah
        /// - header surah
        /// - iluminasi awal surah
        /// </summary>
        public bool IsStartOfSurah { get; set; }

        /// <summary>
        /// Penanda awal juz.
        ///
        /// Belum digunakan untuk layout pada STEP 25,
        /// tetapi disiapkan untuk tahap berikutnya.
        /// </summary>
        public bool IsStartOfJuz { get; set; }

        /// <summary>
        /// Slot teks Arab footer.
        ///
        /// Sengaja dipisahkan dari teks ayat supaya
        /// footer/ornamen dapat dikembangkan tanpa
        /// mengubah engine utama.
        /// </summary>
        public string FooterArabicText { get; set; } =
            string.Empty;
        /// <summary>
        /// Template halaman mushaf.
        /// Default diset ke Standard dan dapat diubah oleh layanan halaman.
        /// </summary>
        public QuranPageTemplate Template { get; set; } = QuranPageTemplate.Standard;
        /// <summary>
        /// Informasi header dinamis yang akan digunakan untuk merender
        /// bagian header halaman (nama surat, juz, dll.).
        /// </summary>
        public QuranPageHeader Header { get; set; } = new QuranPageHeader();

        /// <summary>
        /// Informasi footer dinamis untuk halaman ini.
        /// </summary>
        public QuranPageFooter Footer { get; set; } = new QuranPageFooter();

        /// <summary>
        /// Daftar informasi surah-start yang terdeteksi pada halaman.
        /// Berguna untuk menentukan apakah halaman benar-benar memulai surah
        /// atau hanya memiliki pergantian surah di tengah.
        /// </summary>
        public IReadOnlyList<SurahStartInfo> SurahStarts { get; set; } = new List<SurahStartInfo>();
        /// <summary>
        /// Koleksi section surah pada halaman yang akan digunakan untuk
        /// merender Bismillah, nama surah, dan iluminasi.
        /// </summary>
        public IReadOnlyList<SurahSection> SurahSections { get; set; } = new List<SurahSection>();


    }
}
