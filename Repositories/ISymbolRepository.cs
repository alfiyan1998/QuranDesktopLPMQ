using System.Collections.Generic;
using System.Threading.Tasks;
using QuranKemenag.Models;

namespace QuranKemenag.Repositories;

/// <summary>
/// Repository interface for reading symbol data from the secondary symbol database (msi_simbol.db).
/// This interface exposes read-only operations required by the UI/rendering layer.
/// </summary>
public interface ISymbolRepository
{
    /// <summary>
    /// Retrieves all symbol entries (AyatSymbol) for the specified surah number.
    /// </summary>
    /// <param name="surahNumber">Nomor surah (1..114).</param>
    /// <returns>Read-only list of <see cref="AyatSymbol"/> entries for the surah.</returns>
    Task<IReadOnlyList<AyatSymbol>> GetAyatBySurahAsync(int surahNumber);

    Task<IReadOnlyList<AyatSymbol>> GetAyatByPageAsync(
    int pageNumber);

    /// <summary>
    /// Retrieves all symbol entries (AyatSymbol) for the specified Juz number,
    /// queried directly from msi_simbol.db (kolom 'juz' pada database simbol).
    /// TIDAK melakukan fallback ke database lain — hanya mengembalikan
    /// data yang benar-benar ada di msi_simbol.db.
    /// </summary>
    /// <param name="juzNumber">Nomor Juz (1..30).</param>
    Task<IReadOnlyList<AyatSymbol>> GetAyatByJuzAsync(
    int juzNumber);

    Task<int> GetTotalPagesAsync();

    /// <summary>
    /// Mengambil satu entri simbol berdasarkan surah + ayat.
    /// Dipakai MushafPageService untuk mode Handwritten.
    /// Null jika ayat ini belum ada datanya di msi_simbol.db.
    /// </summary>
    Task<AyatSymbol?> GetAyatAsync(int surahNumber, int ayatNumber);
}