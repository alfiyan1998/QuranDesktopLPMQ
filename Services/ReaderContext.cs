using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using QuranKemenag.Models;

namespace QuranKemenag.Services;

/// <summary>
/// Context yang menjadi penghubung antara
/// MainViewModel dan ReaderServiceFactory.
///
/// MainViewModel tidak perlu mengetahui
/// implementasi reader Unicode / Symbol.
/// </summary>
public sealed class ReaderContext
{
    private readonly ReaderServiceFactory _factory;

    /// <summary>
    /// Mode reader yang sedang aktif.
    /// </summary>
    public ReaderMode CurrentMode { get; private set; }
        = ReaderMode.Unicode;


    public ReaderContext(
        ReaderServiceFactory factory)
    {
        _factory =
            factory
            ?? throw new ArgumentNullException(
                nameof(factory));
    }


    /// <summary>
    /// Mengubah mode reader.
    /// </summary>
    public void SetMode(
        ReaderMode mode)
    {
        CurrentMode = mode;
    }


    /// <summary>
    /// Mengambil ayat berdasarkan Surah
    /// menggunakan reader aktif.
    /// </summary>
    public async Task<IReadOnlyList<Ayat>> GetAyatAsync(
        int surahNumber)
    {
        var reader =
            _factory.Create(CurrentMode);

        return await reader
            .GetAyatAsync(surahNumber);
    }


    /// <summary>
    /// Mengambil ayat berdasarkan halaman
    /// menggunakan reader aktif.
    /// </summary>
    public async Task<IReadOnlyList<Ayat>> GetAyatByPageAsync(
        int pageNumber)
    {
        var reader =
            _factory.Create(CurrentMode);

        return await reader
            .GetAyatByPageAsync(pageNumber);
    }


    /// <summary>
    /// Mengambil jumlah seluruh halaman Quran.
    /// </summary>
    public async Task<int> GetTotalPageAsync()
    {
        var reader =
            _factory.Create(CurrentMode);

        return await reader
            .GetTotalPageAsync();
    }

    public async Task<IReadOnlyList<Ayat>> GetAyatByJuzAsync(
        int juzNumber)
    {
        var reader =
            _factory.Create(CurrentMode);
        return await reader
            .GetAyatByJuzAsync(juzNumber)
            .ConfigureAwait(false);
    }
}