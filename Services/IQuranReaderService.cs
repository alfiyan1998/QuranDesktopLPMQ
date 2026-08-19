using System.Collections.Generic;
using System.Threading.Tasks;

using QuranKemenag.Models;

namespace QuranKemenag.Services;

public interface IQuranReaderService
{
    Task<IReadOnlyList<Ayat>> GetAyatAsync(
        int surahNumber);

    Task<IReadOnlyList<Ayat>> GetAyatByPageAsync(
        int pageNumber);

    Task<int> GetTotalPageAsync();

    Task<IReadOnlyList<Ayat>> GetAyatByJuzAsync(
        int juzNumber);
}