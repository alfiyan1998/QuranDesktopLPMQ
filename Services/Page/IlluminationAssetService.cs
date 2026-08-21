using System.Collections.Generic;
using System.Diagnostics;
using QuranKemenag.Models;

namespace QuranKemenag.Services.Page
{
    /// <summary>
    /// Service that selects illumination assets for page sections.
    /// Does not perform rendering; only maps sections to asset paths.
    /// </summary>
    public sealed class IlluminationAssetService
    {
        public IlluminationAsset? GetAssetForSection(SurahSectionLayout section)
        {
            if (section is null)
                return null;

            if (IlluminationAssetRegistry.TryGetAsset(section.LayoutType, out var asset))
            {
                Debug.WriteLine($"[ILLUMINATION] Surah={section.SurahNumber} Type={section.LayoutType} Asset={asset.FilePath}");
                return asset;
            }

            Debug.WriteLine($"[ILLUMINATION] Surah={section.SurahNumber} Type={section.LayoutType} Asset=NONE");
            return null;
        }

        public IReadOnlyList<(SurahSectionLayout Section, IlluminationAsset? Asset)> BuildAssetsForPage(MushafPageLayout layout)
        {
            var list = new List<(SurahSectionLayout, IlluminationAsset?)>();
            if (layout?.Sections == null)
                return list;

            foreach (var s in layout.Sections)
            {
                var asset = GetAssetForSection(s);
                Debug.WriteLine($"[ILLUMINATION] Page={layout.PageNumber} Surah={s.SurahNumber} Type={s.LayoutType} Asset={(asset?.FilePath ?? "NONE")}");
                list.Add((s, asset));
            }

            return list;
        }
    }
}
