using QuranKemenag.Models;
using QuranKemenag.Models.Copy;
using QuranKemenag.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuranKemenag.Services.Copy;

public sealed class QuranCopyService
{
    private readonly ReaderContext _readerContext;
    private readonly ITranslationRepository _translationRepository;

    public QuranCopyService(
        ReaderContext readerContext,
        ITranslationRepository translationRepository)
    {
        _readerContext =
            readerContext
            ?? throw new ArgumentNullException(
                nameof(readerContext));

        _translationRepository =
            translationRepository
            ?? throw new ArgumentNullException(
                nameof(translationRepository));
    }

    /// <summary>
    /// Mengambil range ayat untuk kebutuhan Copy.
    ///
    /// Contoh:
    /// Al-Fatihah 1 → Al-Baqarah 5
    ///
    /// Hasil:
    /// Al-Fatihah 1-7
    /// Al-Baqarah 1-5
    ///
    /// Jika range terbalik:
    /// Al-Baqarah 1 → Al-Fatihah 3
    ///
    /// otomatis dinormalisasi menjadi:
    /// Al-Fatihah 3 → Al-Baqarah 1
    /// </summary>
    public async Task<IReadOnlyList<Ayat>> GetRangeAsync(
        int fromSurah,
        int fromAyah,
        int toSurah,
        int toAyah,
        ArabicCopyFont font)
    {
        // =========================================================
        // 1. VALIDASI PARAMETER
        // =========================================================

        if (fromSurah <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(fromSurah));

        if (fromAyah <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(fromAyah));

        if (toSurah <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(toSurah));

        if (toAyah <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(toAyah));

        // =========================================================
        // 2. NORMALISASI RANGE
        // =========================================================
        //
        // Contoh:
        //
        // FROM = Al-Baqarah 1
        // TO   = Al-Fatihah 3
        //
        // menjadi:
        //
        // FROM = Al-Fatihah 3
        // TO   = Al-Baqarah 1
        //
        // =========================================================

        if (fromSurah > toSurah ||
            (fromSurah == toSurah &&
             fromAyah > toAyah))
        {
            (
                fromSurah,
                toSurah
            ) =
            (
                toSurah,
                fromSurah
            );

            (
                fromAyah,
                toAyah
            ) =
            (
                toAyah,
                fromAyah
            );
        }

        // =========================================================
        // 3. TENTUKAN MODE ARAB
        // =========================================================

        var readerMode =
            font == ArabicCopyFont.Handwritten
                ? ReaderMode.Symbol
                : ReaderMode.Unicode;

        // Simpan mode aplikasi sebelum proses copy.
        var previousMode =
            _readerContext.CurrentMode;

        try
        {
            // =====================================================
            // 4. AKTIFKAN MODE SESUAI FONT YANG DIPILIH USER
            // =====================================================

            _readerContext.SetMode(
                readerMode);

            var result =
                new List<Ayat>();

            // =====================================================
            // 5. AMBIL SETIAP SURAH DALAM RANGE
            // =====================================================

            for (
                int surahNumber = fromSurah;
                surahNumber <= toSurah;
                surahNumber++)
            {
                var ayats =
                    await _readerContext
                        .GetAyatAsync(
                            surahNumber);

                if (ayats is null ||
                    ayats.Count == 0)
                {
                    continue;
                }

                // =================================================
                // 6. TENTUKAN BATAS AYAT
                // =================================================

                int firstAyah =
                    surahNumber == fromSurah
                        ? fromAyah
                        : 1;

                int lastAyah =
                    surahNumber == toSurah
                        ? toAyah
                        : int.MaxValue;

                // =================================================
                // 7. FILTER AYAT
                // =================================================

                var selectedAyats =
                    ayats
                        .Where(a =>
                            a.AyatNumber >= firstAyah &&
                            a.AyatNumber <= lastAyah)
                        .OrderBy(a => a.AyatNumber)
                        .ToList();

                // =================================================
                // 8. AMBIL TERJEMAHAN SETIAP AYAT
                // =================================================
                //
                // Repository project Anda menggunakan:
                //
                // GetTranslationAsync(
                //     surahNumber,
                //     ayatNumber)
                //
                // bukan GetTranslationsBySurahAsync().
                //
                // =================================================

                foreach (var ayat in selectedAyats)
                {
                    var translation =
                        await _translationRepository
                            .GetTranslationAsync(
                                surahNumber,
                                ayat.AyatNumber);

                    // =================================================
                    // 9. MASUKKAN TERJEMAHAN
                    // =================================================

                    if (translation is not null)
                    {
                        ayat.Terjemahan =
                            translation.Text;

                        ayat.NoFn =
                            translation.NoFn;

                        ayat.TextFn =
                            translation.TextFn
                            ?? string.Empty;
                    }
                    else
                    {
                        // Pastikan tidak membawa nilai lama.
                        ayat.Terjemahan =
                            string.Empty;

                        ayat.NoFn =
                            null;

                        ayat.TextFn =
                            string.Empty;
                    }

                    result.Add(ayat);
                }
            }

            // =====================================================
            // 10. URUTKAN HASIL FINAL
            // =====================================================

            return result
                .OrderBy(a => a.Surah)
                .ThenBy(a => a.AyatNumber)
                .ToList();
        }
        finally
        {
            // =====================================================
            // 11. KEMBALIKAN MODE READER
            // =====================================================
            //
            // Misalnya sebelum copy aplikasi sedang:
            //
            // Unicode
            //
            // User memilih:
            //
            // Handwritten
            //
            // Setelah proses selesai, ReaderContext dikembalikan
            // ke mode sebelumnya.
            //
            // =====================================================

            _readerContext.SetMode(
                previousMode);
        }
    }
}