using System.Collections.Generic;
using System.Threading.Tasks;
using QuranKemenag.Models;

namespace QuranKemenag.Repositories;

/// <summary>
/// Kontrak repository untuk database Quran Unicode.
/// </summary>
public interface IQuranRepository
{
    /// <summary>
    /// Mengambil seluruh daftar Surah.
    /// </summary>
    Task<IReadOnlyList<Surah>> GetAllSurahAsync();

    /// <summary>
    /// Mengambil seluruh ayat berdasarkan nomor Surah.
    /// </summary>
    Task<IReadOnlyList<Ayat>> GetAyatBySurahAsync(
        int surahNumber);

    /// <summary>
    /// Mengambil seluruh ayat berdasarkan nomor halaman Mushaf.
    /// </summary>
    Task<IReadOnlyList<Ayat>> GetAyatByPageAsync(
        int pageNumber);

    /// <summary>
    /// Mengambil jumlah seluruh halaman Quran.
    /// </summary>
    Task<int> GetTotalPageAsync();

    Task<JuzStartInfo?>GetJuzStartAsync(int juzNumber);

    Task<IReadOnlyList<Ayat>> GetAyatByJuzAsync(int juzNumber);

    /// <summary>
    /// Mengambil satu ayat lengkap berdasarkan nomor surah dan ayat.
    /// Dipakai oleh MushafPageService untuk mendapatkan teks Unicode
    /// asli (kolom 'teks') yang kemudian dipecah per kata.
    /// </summary>
    Task<Ayat?> GetAyatAsync(int surahNumber, int ayatNumber);
}
