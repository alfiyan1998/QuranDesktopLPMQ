// csharp
using System;
using QuranKemenag.Helpers;

namespace QuranKemenag.Models;

/// <summary>
/// Represents a single record from the "quran" table in the SQLite database.
/// This model follows the database column names and types so Dapper can map
/// query results directly to this class.
/// </summary>
public class Ayat
{
    /// <summary>
    /// Primary identifier of the record (maps to column 'id').
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nomor surah (maps to column 'surah'). Contoh: 1 untuk Al-Fatihah.
    /// </summary>
    public int Surah { get; set; }

    /// <summary>
    /// Nomor ayat di dalam surah (maps to column 'ayat').
    /// Nama properti menggunakan 'AyatNumber' karena nama kelas adalah 'Ayat'.
    /// </summary>
    public int AyatNumber { get; set; }

    /// <summary>
    /// Juz (maps to column 'juz'). Menunjukkan bagian juz tempat ayat berada.
    /// </summary>
    public int Juz { get; set; }

    /// <summary>
    /// Teks lengkap ayat (maps to column 'teks'). Diinisialisasi ke string.Empty.
    /// </summary>
    public string Teks { get; set; } = string.Empty;

    /// <summary>
    /// Teks gundul (tanpa harakat) dari ayat (maps to column 'gundul').
    /// Diinisialisasi ke string.Empty.
    /// </summary>
    public string Gundul { get; set; } = string.Empty;

    /// <summary>
    /// Kolom isyarat (maps to column 'isyarat'), biasanya berisi tanda atau metadata.
    /// Diinisialisasi ke string.Empty.
    /// </summary>
    public string Isyarat { get; set; } = string.Empty;

    // ------------------------------------------------------------------
    // Legacy compatibility properties
    // ------------------------------------------------------------------
    // The repository and some parts of the app previously used different
    // property names (AyahNumber, ArabicText). To avoid breaking existing
    // code while migrating the model to match the database, we provide
    // simple compatibility wrappers that forward to the new properties.
    // These are marked Obsolete to encourage updating callers.

    /// <summary>
    /// Kode font yang digunakan untuk merender ayat pada mode Symbol.
    /// Nilai berasal dari database msi_simbol.db.
    /// </summary>
    public int KodeFont { get; set; }

    /// <summary>
    /// Nomor ayat dalam angka Arab.
    /// Contoh:
    /// 1 -> ١
    /// 25 -> ٢٥
    /// </summary>
    public string ArabicAyatNumber =>
        ArabicNumberHelper.ToArabicNumber(AyatNumber);
    public string MushafAyatNumber =>
   $"۝{ArabicAyatNumber}";

    /// <summary>
    /// Teks yang akan ditampilkan pada Reader.
    /// Unicode  : Teks + nomor ayat.
    /// Handwritten : hanya teks.
    /// </summary>
    public string DisplayText => Teks;


    /// <summary>
    /// Legacy property name: maps to <see cref="Ayat"/>. Keberadaan properti
    /// ini mempermudah kompatibilitas sementara. Gunakan <see cref="Ayat"/>.
    /// </summary>
    [Obsolete("Use AyatNumber property instead.")]
    public int AyahNumber
    {
        get => AyatNumber;
        set => AyatNumber = value;
    }

    /// <summary>
    /// Legacy property name: maps to <see cref="Teks"/>. Gunakan <see cref="Teks"/>.
    /// </summary>
    [Obsolete("Use Teks property instead.")]
    public string ArabicText
    {
        get => Teks;
        set => Teks = value ?? string.Empty;
    }

    /// <summary>
    /// Terjemahan Bahasa Indonesia untuk ayat.
    /// </summary>
    public string Terjemahan { get; set; } = string.Empty;

    /// <summary>
    /// Nomor catatan kaki dari database Indonesia2019.
    /// SQLite INTEGER dibaca sebagai Int64.
    /// </summary>
    public string? NoFn { get; set; }

    /// <summary>
    /// Isi catatan kaki dari database Indonesia2019.
    /// </summary>
    public string TextFn { get; set; } = string.Empty;


    /// <summary>
    /// Nomor halaman mushaf tempat ayat berada.
    /// Nullable karena database lama belum memiliki data halaman.
    /// </summary>
    public int? Halaman { get; set; }


    /// <summary>
    /// Teks terjemahan Bahasa Indonesia.
    /// Data berasal dari database Indonesia2019.
    /// </summary>
    //public string Translation { get; set; } = string.Empty;

    /// <summary>
    /// Nomor footnote dari database terjemahan.
    /// Contoh: 1, 2, 3.
    /// </summary>
    //public string TranslationFootnoteNumber { get; set; } = string.Empty;

    /// <summary>
    /// Isi footnote dari database terjemahan.
    /// </summary>
    //public string TranslationFootnote { get; set; } = string.Empty;

    /// <summary>
    /// Menandakan bahwa ayat ini adalah awal sebuah Surah.
    /// Digunakan oleh Mode Per Ayat untuk menampilkan
    /// header iluminasi + nama Surah.
    /// </summary>
    public bool IsSurahStart { get; set; }

    /// <summary>
    /// Nama Surah dalam bahasa Arab.
    /// </summary>
    public string SurahArabicName { get; set; } = string.Empty;

    /// <summary>
    /// Kategori Surah dalam bahasa Arab.
    /// Contoh: مكية / مدنية
    /// </summary>
    public string SurahCategoryArabic { get; set; } = string.Empty;

    /// <summary>
    /// Jumlah ayat Surah dalam angka Arab.
    /// </summary>
    public string SurahTotalAyahArabic { get; set; } = string.Empty;

    /// <summary>
    /// Bismillah ditampilkan atau tidak.
    /// At-Taubah (Surah 9) tidak memiliki Bismillah.
    /// </summary>
    public bool ShowBismillah { get; set; }


}
