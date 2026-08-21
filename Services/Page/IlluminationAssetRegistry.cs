using System.Collections.Generic;
using QuranKemenag.Models;

namespace QuranKemenag.Services.Page
{
    /// <summary>
    /// Central registry for illumination asset file paths.
    /// Allows mapping SurahSectionLayoutType -> asset file.
    /// </summary>
    public static class IlluminationAssetRegistry
    {
        private static readonly Dictionary<SurahSectionLayoutType, IlluminationAsset> _map
            = new()
            {
                { SurahSectionLayoutType.AlFatiha, new IlluminationAsset { Category = "AlFatiha", FilePath = "Assets/Illuminations/AlFatiha/alfatiha.png" } },
                { SurahSectionLayoutType.AlifLamMim, new IlluminationAsset { Category = "AlifLamMim", FilePath = "Assets/Illuminations/AlifLamMim/alif_lam_mim.png" } },
                { SurahSectionLayoutType.StandardStart, new IlluminationAsset { Category = "Standard", FilePath = "Assets/Illuminations/Standard/standard_start.png" } },
                { SurahSectionLayoutType.MiddleSwitch, new IlluminationAsset { Category = "Middle", FilePath = "Assets/Illuminations/Middle/middle_switch.png" } }
                // SurahSectionLayoutType.Standard intentionally has no asset mapping.
            };

        public static bool TryGetAsset(SurahSectionLayoutType type, out IlluminationAsset? asset)
        {
            return _map.TryGetValue(type, out asset);
        }
    }
}
