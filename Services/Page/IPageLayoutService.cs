using System.Collections.Generic;
using QuranKemenag.Models;

namespace QuranKemenag.Services.Page
{
    public interface IPageLayoutService
    {
        MushafPageLayout BuildLayout(QuranPage page);

        IReadOnlyList<MushafPageLayout> BuildLayouts(IReadOnlyList<QuranPage> pages);
    }
}
