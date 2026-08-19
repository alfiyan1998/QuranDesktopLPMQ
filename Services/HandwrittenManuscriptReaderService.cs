using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuranKemenag.Models;
using QuranKemenag.Repositories;
using QuranKemenag.Helpers;
using QuranKemenag.Services.Text;

namespace QuranKemenag.Services;

/// <summary>
/// Reader service for Handwritten Manuscript display mode.
///
/// ATURAN SUMBER DATA (PENTING):
/// Mode Handwritten HANYA menampilkan ayat yang benar-benar ada di
/// msi_simbol.db. TIDAK ADA fallback ke quran.kemenag untuk mengisi
/// ayat yang belum tersedia.
///
/// Contoh:
/// Jika msi_simbol.db baru punya Al-Baqarah ayat 1-5, maka membuka
/// Al-Baqarah / Juz 1 dalam mode Handwritten HANYA menampilkan
/// ayat 1-5. Ayat 6 dst TIDAK ditampilkan (bukan diambil dari
/// quran.kemenag) sampai datanya tersedia di msi_simbol.db.
///
/// quran.kemenag hanya dipakai untuk melengkapi metadata pada ayat
/// yang SUDAH ADA di msi_simbol.db (Isyarat), bukan untuk menambah
/// ayat baru ke daftar.
/// </summary>
public sealed class HandwrittenManuscriptReaderService : IQuranReaderService
{
    private readonly ISymbolRepository _repository;
    private readonly ISymbolTextProcessor _processor;
    private readonly IQuranRepository _quranRepository;

    public HandwrittenManuscriptReaderService(
     ISymbolRepository repository,
     ISymbolTextProcessor processor,
     IQuranRepository quranRepository)
    {
        _repository = repository;
        _processor = processor;
        _quranRepository = quranRepository;
    }

    public async Task<IReadOnlyList<Ayat>> GetAyatAsync(int surahNumber)
    {
        Debug.WriteLine("--------------------------------");
        Debug.WriteLine("HandwrittenManuscriptReaderService.GetAyatAsync");
        Debug.WriteLine($"Surah = {surahNumber}");
        Debug.WriteLine("--------------------------------");

        // =====================================================
        // SUMBER DATA UTAMA: msi_simbol.db.
        // Hanya ayat yang ada di sini yang akan ditampilkan.
        // =====================================================

        var symbols =
            await _repository
                .GetAyatBySurahAsync(surahNumber)
                .ConfigureAwait(false);

        var ayats =
            _processor.Process(symbols);

        // =====================================================
        // ISYARAT: metadata tambahan dari quran.kemenag,
        // hanya untuk melengkapi ayat yang SUDAH ADA di atas.
        // TIDAK menambah ayat baru ke daftar.
        // =====================================================

        await AttachIsyaratAsync(ayats, surahNumber)
            .ConfigureAwait(false);

        Debug.WriteLine(
            $"[SURAH] Handwritten ayat = {ayats.Count}");

        return ayats;
    }

    public async Task<IReadOnlyList<Ayat>> GetAyatByPageAsync(
    int pageNumber)
    {
        Debug.WriteLine("--------------------------------");
        Debug.WriteLine("HandwrittenManuscriptReaderService.GetAyatByPageAsync");
        Debug.WriteLine($"Halaman = {pageNumber}");
        Debug.WriteLine("--------------------------------");

        // =====================================================
        // SUMBER DATA UTAMA: msi_simbol.db.
        // =====================================================

        var symbols =
            await _repository
                .GetAyatByPageAsync(pageNumber)
                .ConfigureAwait(false);

        var ayats =
            _processor.Process(symbols);

        // =====================================================
        // ISYARAT: lengkapi dari quran.kemenag berdasarkan
        // Surah + Nomor Ayat yang SUDAH ADA di hasil di atas.
        // =====================================================

        var surahNumbers =
            ayats
                .Select(x => x.Surah)
                .Distinct()
                .ToList();

        foreach (var surahNumber in surahNumbers)
        {
            await AttachIsyaratAsync(
                    ayats.Where(x => x.Surah == surahNumber).ToList(),
                    surahNumber)
                .ConfigureAwait(false);
        }

        Debug.WriteLine(
            $"[PAGE] Handwritten ayat = {ayats.Count}");

        return ayats;
    }

    public async Task<int> GetTotalPageAsync()
    {
        return await _quranRepository
            .GetTotalPageAsync();
    }

    public async Task<IReadOnlyList<Ayat>> GetAyatByJuzAsync(
    int juzNumber)
    {
        Debug.WriteLine(
            $"[JUZ] HandwrittenManuscriptReaderService - Juz={juzNumber}");

        // =====================================================
        // SUMBER DATA UTAMA: msi_simbol.db, query LANGSUNG
        // berdasarkan kolom 'juz' pada database simbol itu
        // sendiri (BUKAN driven oleh quran.kemenag).
        //
        // Jika msi_simbol.db belum punya data untuk Juz ini
        // (sebagian atau seluruhnya), ayat yang hilang TIDAK
        // ditampilkan.
        // =====================================================

        var symbols =
            await _repository
                .GetAyatByJuzAsync(juzNumber)
                .ConfigureAwait(false);

        if (symbols is null ||
            symbols.Count == 0)
        {
            Debug.WriteLine(
                $"[JUZ] msi_simbol.db belum memiliki data " +
                $"untuk Juz {juzNumber}.");

            return Array.Empty<Ayat>();
        }

        var ayats =
            _processor.Process(symbols)
                .OrderBy(x => x.Surah)
                .ThenBy(x => x.AyatNumber)
                .ToList();

        // =====================================================
        // ISYARAT: lengkapi dari quran.kemenag per Surah,
        // hanya untuk ayat yang SUDAH ADA di hasil di atas.
        // =====================================================

        var surahNumbers =
            ayats
                .Select(x => x.Surah)
                .Distinct()
                .ToList();

        foreach (var surahNumber in surahNumbers)
        {
            await AttachIsyaratAsync(
                    ayats.Where(x => x.Surah == surahNumber).ToList(),
                    surahNumber)
                .ConfigureAwait(false);
        }

        Debug.WriteLine(
            $"[JUZ] Handwritten ayat = {ayats.Count}");

        return ayats;
    }

    /// <summary>
    /// Melengkapi properti Isyarat pada daftar Ayat yang SUDAH ADA
    /// (hasil dari msi_simbol.db), berdasarkan data quran.kemenag
    /// untuk Surah yang sama. Method ini TIDAK PERNAH menambah
    /// ayat baru ke <paramref name="ayats"/>.
    /// </summary>
    private async Task AttachIsyaratAsync(
        IReadOnlyList<Ayat> ayats,
        int surahNumber)
    {
        if (ayats.Count == 0)
            return;

        var unicodeAyats =
            await _quranRepository
                .GetAyatBySurahAsync(surahNumber)
                .ConfigureAwait(false);

        var isyaratMap =
            unicodeAyats.ToDictionary(
                x => x.AyatNumber,
                x => x.Isyarat ?? string.Empty);

        foreach (var ayat in ayats)
        {
            if (isyaratMap.TryGetValue(
                ayat.AyatNumber,
                out var isyarat))
            {
                ayat.Isyarat = isyarat;
            }
        }
    }
}