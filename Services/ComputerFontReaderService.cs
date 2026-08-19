
using QuranKemenag.Models;
using QuranKemenag.Repositories;

namespace QuranKemenag.Services;

/// <summary>
/// Service pembaca Al-Qur'an menggunakan database Unicode.
/// </summary>
public sealed class ComputerFontReaderService : IQuranReaderService
{
    private readonly IQuranRepository _repository;

    public ComputerFontReaderService(IQuranRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<Ayat>> GetAyatAsync(int surahNumber)
    {
        System.Diagnostics.Debug.WriteLine("--------------------------------");
        System.Diagnostics.Debug.WriteLine("ComputerFontReaderService");
        System.Diagnostics.Debug.WriteLine("--------------------------------");

        var ayats = await _repository.GetAyatBySurahAsync(surahNumber);

        //foreach (var ayat in ayats)
        //{
           
        //    System.Diagnostics.Debug.WriteLine(
        //        $"[Unicode] Ayat={ayat.AyatNumber}");
        //}

        return ayats;
    }

    public async Task<IReadOnlyList<Ayat>> GetAyatByPageAsync(
    int pageNumber)
    {
        var ayats =
            await _repository
                .GetAyatByPageAsync(
                    pageNumber);

        return ayats;
    }


    public async Task<int> GetTotalPageAsync()
    {
        return await _repository
            .GetTotalPageAsync();
    }

    public async Task<IReadOnlyList<Ayat>> GetAyatByJuzAsync(
        int juzNumber)
    {
        
           return await _repository
                .GetAyatByJuzAsync(juzNumber)
                .ConfigureAwait(false);

       
    }
}