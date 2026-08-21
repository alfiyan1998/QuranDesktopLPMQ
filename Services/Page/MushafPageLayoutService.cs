using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using QuranKemenag.Models;

namespace QuranKemenag.Services.Page
{
    /// <summary>
    /// Service untuk menentukan layout struktur halaman mushaf.
    /// Tidak melakukan rendering, hanya menentukan susunan komponen.
    /// </summary>
    public sealed class MushafPageLayoutService : IPageLayoutService
    {
        public MushafPageLayout BuildLayout(QuranPage page)
        {
            if (page is null)
                return null!;

            // Build section layouts from SurahSections if present, otherwise derive from SurahStarts
            var sections = (page.SurahSections?.Any() == true)
                ? page.SurahSections.Select(s => BuildSectionLayoutFromSection(s)).ToList()
                : (page.SurahStarts?.Any() == true)
                    ? page.SurahStarts.Select(s => BuildSectionLayoutFromStart(s)).ToList()
                    : new List<SurahSectionLayout>();

            var layout = new MushafPageLayout
            {
                PageNumber = page.PageNumber,
                Template = page.Template,
                Header = page.Header ?? new QuranPageHeader { PageNumber = page.PageNumber },
                Sections = sections,
                AyatCount = page.Ayats?.Count ?? 0,
                Footer = page.Footer ?? new QuranPageFooter { PageNumber = page.PageNumber }
            };

            // Debug output as requested
            var headerName = layout.Header.PrimarySurahLatinName ?? string.Empty;
            Debug.WriteLine($"[LAYOUT] Page={layout.PageNumber}");
            Debug.WriteLine($"[LAYOUT] Template={layout.Template}");
            Debug.WriteLine($"[LAYOUT] Header={headerName} Juz={layout.Header.Juz}");
            Debug.WriteLine($"[LAYOUT] Sections={layout.Sections?.Count}");
            Debug.WriteLine($"[LAYOUT] AyatCount={layout.AyatCount}");
            Debug.WriteLine($"[LAYOUT] Footer={layout.Footer?.PageNumber}");

            // Detail sections
            if (layout.Sections?.Any() == true)
            {
                foreach (var s in layout.Sections)
                {
                    Debug.WriteLine($"[LAYOUT] Section Surah={s.SurahNumber} Ayat={s.StartAyatNumber} Position={s.Position} Type={s.LayoutType} Bismillah={s.ShowBismillah}");
                }
            }

            return layout;
        }

        public IReadOnlyList<MushafPageLayout> BuildLayouts(IReadOnlyList<QuranPage> pages)
        {
            if (pages is null || pages.Count == 0)
                return new List<MushafPageLayout>();

            return pages.Select(BuildLayout).ToList();
        }

        private static SurahSectionLayout BuildSectionLayoutFromSection(SurahSection s)
        {
            var type = DetermineLayoutType(s);
            return new SurahSectionLayout
            {
                SurahNumber = s.SurahNumber,
                StartAyatNumber = s.StartAyatNumber,
                Position = s.Position,
                ShowBismillah = s.ShowBismillah,
                IsAlFatiha = s.IsAlFatiha,
                IsAlifLamMim = s.IsAlifLamMim,
                LayoutType = type
            };
        }

        private static SurahSectionLayout BuildSectionLayoutFromStart(SurahStartInfo s)
        {
            var isAlFatiha = s.SurahNumber == 1;
            var isAlifLamMim = s.SurahNumber == 2 && s.AyatNumber == 1;
            var showBismillah = s.AyatNumber == 1 && s.SurahNumber != 9; // At-Taubah exception
            var type = SurahSectionLayoutType.Standard;

            if (isAlFatiha)
                type = SurahSectionLayoutType.AlFatiha;
            else if (isAlifLamMim)
                type = SurahSectionLayoutType.AlifLamMim;
            else if (s.Position == SurahStartPosition.Start && s.AyatNumber == 1)
                type = SurahSectionLayoutType.StandardStart;
            else if (s.Position == SurahStartPosition.Middle)
                type = SurahSectionLayoutType.MiddleSwitch;

            return new SurahSectionLayout
            {
                SurahNumber = s.SurahNumber,
                StartAyatNumber = s.AyatNumber,
                Position = s.Position,
                ShowBismillah = showBismillah,
                IsAlFatiha = isAlFatiha,
                IsAlifLamMim = isAlifLamMim,
                LayoutType = type
            };
        }

        private static SurahSectionLayoutType DetermineLayoutType(SurahSection s)
        {
            if (s.IsAlFatiha)
                return SurahSectionLayoutType.AlFatiha;
            if (s.IsAlifLamMim)
                return SurahSectionLayoutType.AlifLamMim;
            if (s.Position == SurahStartPosition.Middle)
                return SurahSectionLayoutType.MiddleSwitch;
            if (s.IsFirstPageOfSurah)
                return SurahSectionLayoutType.StandardStart;
            return SurahSectionLayoutType.Standard;
        }
    }
}
