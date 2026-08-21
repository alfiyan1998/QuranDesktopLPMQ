namespace QuranKemenag.Models
{
    /// <summary>
    /// Represents an illumination asset mapping.
    /// </summary>
    public sealed class IlluminationAsset
    {
        /// <summary>
        /// Logical category (AlFatiha, AlifLamMim, Standard, Middle)
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Relative path to asset file from application root.
        /// Example: Assets/Illuminations/AlFatiha/alfatiha.png
        /// </summary>
        public string FilePath { get; set; } = string.Empty;
    }
}
