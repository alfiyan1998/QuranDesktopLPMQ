using QuranKemenag.Models;

namespace QuranKemenag.Services.Page;

/// <summary>
/// Menyediakan fungsi untuk membentuk dan mengambil
/// struktur halaman mushaf dari kumpulan ayat.
/// </summary>
public interface IQuranPageService
{
    /// <summary>
    /// Mengelompokkan ayat berdasarkan nomor halaman.
    /// </summary>
    IReadOnlyList<QuranPage> BuildPages(
    IReadOnlyList<Ayat> ayats,
    IReadOnlyList<Surah>? surahs = null);

    /// <summary>
    /// Mengambil satu halaman berdasarkan nomor halaman.
    /// </summary>
    QuranPage? GetPage(
        IReadOnlyList<QuranPage> pages,
        int pageNumber);


}