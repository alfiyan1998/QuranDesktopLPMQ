using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using QuranKemenag.Models;

namespace QuranKemenag.Services.Page;

public sealed class QuranPageService : IQuranPageService
{
    public IReadOnlyList<QuranPage> BuildPages(
        IReadOnlyList<Ayat> ayats,
        IReadOnlyList<Surah>? surahs = null)
    {
        if (ayats is null || ayats.Count == 0)
            return System.Array.Empty<QuranPage>();

        var pages = ayats
            .Where(x => x.Halaman.HasValue)
            .GroupBy(x => x.Halaman!.Value)
            .OrderBy(x => x.Key)
            .Select(group =>
            {
                var pageAyats = group
                    .OrderBy(x => x.Surah)
                    .ThenBy(x => x.AyatNumber)
                    .ToList();

                var firstAyat =
                    pageAyats.FirstOrDefault();

                var firstSurah =
                    firstAyat is null || surahs is null
                        ? null
                        : surahs.FirstOrDefault(
                            x => x.Id == firstAyat.Surah);

                // Prepare header/footer information for the page
                var surahNumbers = pageAyats
                    .Select(a => a.Surah)
                    .Distinct()
                    .ToList();

                var isSurahStart = pageAyats.Any(a => a.AyatNumber == 1);

                // Detect surah change / start positions on this page
                var surahStarts = new List<SurahStartInfo>();
                int? prevSurah = null;
                for (int i = 0; i < pageAyats.Count; i++)
                {
                    var a = pageAyats[i];
                    if (prevSurah is null || a.Surah != prevSurah)
                    {
                        surahStarts.Add(new SurahStartInfo
                        {
                            SurahNumber = a.Surah,
                            AyatNumber = a.AyatNumber,
                            Position = i == 0 ? SurahStartPosition.Start : SurahStartPosition.Middle
                        });
                    }
                    prevSurah = a.Surah;
                }

                var header = new QuranPageHeader
                {
                    PageNumber = group.Key,
                    Juz = firstAyat?.Juz,
                    SurahNumbers = surahNumbers,
                    PrimarySurahNumber = firstAyat?.Surah,
                    PrimarySurahLatinName = firstSurah?.LatinName ?? string.Empty,
                    PrimarySurahArabicName = firstSurah?.ArabicName ?? string.Empty,
                    IsSurahStart = isSurahStart
                };

                var footer = new QuranPageFooter
                {
                    PageNumber = group.Key,
                    FooterArabicText = string.Empty
                };

                // Build SurahSections from detected SurahStarts
                var surahSections = surahStarts
                    .Select(s => new SurahSection
                    {
                        SurahNumber = s.SurahNumber,
                        StartAyatNumber = s.AyatNumber,
                        Position = s.Position,
                        // Consider first-page-of-surah when the section is at page start and begins at ayat 1
                        IsFirstPageOfSurah = s.Position == SurahStartPosition.Start && s.AyatNumber == 1,
                        // Show Bismillah for sections that start at ayat 1 except Surah 9 (At-Taubah)
                        ShowBismillah = s.AyatNumber == 1 && s.SurahNumber != 9,
                        IsAlFatiha = s.SurahNumber == 1,
                        IsAlifLamMim = s.SurahNumber == 2 && s.AyatNumber == 1
                    })
                    .ToList();

                return new QuranPage
                {
                    PageNumber = group.Key,

                    Juz = firstAyat?.Juz,

                    SurahNumber =
                        firstAyat?.Surah,

                    SurahLatinName =
                        firstSurah?.LatinName
                        ?? string.Empty,

                    SurahArabicName =
                        firstSurah?.ArabicName
                        ?? string.Empty,

                    IsStartOfSurah =
                        firstAyat?.AyatNumber == 1,

                    // Tentukan template berdasarkan nomor halaman
                    Template = group.Key == 1
                        ? QuranPageTemplate.AlFatiha
                        : QuranPageTemplate.Standard,

                    // Header & Footer dinamis
                    Header = header,
                    Footer = footer,
                    SurahStarts = surahStarts,
                    SurahSections = surahSections,

                    Ayats = pageAyats
                };
            })
            .ToList();

        // Debug: pastikan template untuk halaman 1 dan 2
        var page1 = pages.FirstOrDefault(p => p.PageNumber == 1);
        if (page1 is not null)
            Debug.WriteLine($"[PAGE] Page={page1.PageNumber} Template={page1.Template}");

        var page2 = pages.FirstOrDefault(p => p.PageNumber == 2);
        if (page2 is not null)
            Debug.WriteLine($"[PAGE] Page={page2.PageNumber} Template={page2.Template}");

        // Debug header/footer examples
        var headerPage2 = pages.FirstOrDefault(p => p.PageNumber == 2);
        if (headerPage2 is not null)
        {
            Debug.WriteLine($"[HEADER] Page={headerPage2.PageNumber} Surah={headerPage2.Header.PrimarySurahLatinName} Juz={headerPage2.Header.Juz}");
            Debug.WriteLine($"[HEADER] SurahStart={headerPage2.Header.IsSurahStart}");
            Debug.WriteLine($"[FOOTER] Page={headerPage2.Footer.PageNumber}");
        }

        var headerPage106 = pages.FirstOrDefault(p => p.PageNumber == 106);
        if (headerPage106 is not null)
        {
            Debug.WriteLine($"[HEADER] Page={headerPage106.PageNumber} Surah={headerPage106.Header.PrimarySurahLatinName} Juz={headerPage106.Header.Juz}");
            Debug.WriteLine($"[HEADER] SurahStart={headerPage106.Header.IsSurahStart}");
        }

        // Debug: print surah start/change events for pages
        foreach (var p in pages)
        {
            if (p.SurahStarts is null || p.SurahStarts.Count == 0)
                continue;

            foreach (var s in p.SurahStarts)
            {
                Debug.WriteLine($"[PAGE SURAH] Page={p.PageNumber} Surah={s.SurahNumber} Ayat={s.AyatNumber} Position={s.Position}");
            }
        }

        // Debug: print surah sections with bismillah and template info
        foreach (var p in pages)
        {
            if (p.SurahSections is null || p.SurahSections.Count == 0)
                continue;

            foreach (var sec in p.SurahSections)
            {
                Debug.WriteLine($"[SURAH SECTION] Page={p.PageNumber} Surah={sec.SurahNumber} Ayat={sec.StartAyatNumber} Position={sec.Position} Bismillah={sec.ShowBismillah} Template={p.Template}");
            }
        }

        return pages;
    }


    public QuranPage? GetPage(
        IReadOnlyList<QuranPage> pages,
        int pageNumber)
    {
        if (pages is null || pages.Count == 0)
            return null;

        return pages.FirstOrDefault(
            x => x.PageNumber == pageNumber);
    }
}