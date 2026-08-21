using System.Collections.Generic;

namespace QuranKemenag.Models
{
    public sealed class MushafPageLayout
    {
        public int PageNumber { get; init; }

        public QuranPageTemplate Template { get; init; }

        public QuranPageHeader Header { get; init; } = new QuranPageHeader();

        public IReadOnlyList<SurahSectionLayout> Sections { get; init; } = new List<SurahSectionLayout>();

        public int AyatCount { get; init; }

        public QuranPageFooter Footer { get; init; } = new QuranPageFooter();
    }
}
