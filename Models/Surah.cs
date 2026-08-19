// csharp
namespace QuranKemenag.Models;

/// <summary>
/// Represents a surah (chapter) metadata from the namasurah table.
/// </summary>
public class Surah
{
    /// <summary>
    /// Primary identifier of the surah (maps to id).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Latin name of the surah (maps to namalatin).
    /// </summary>
    public string LatinName { get; set; } = string.Empty;

    /// <summary>
    /// Arabic name of the surah (maps to namaarab).
    /// </summary>
    public string ArabicName { get; set; } = string.Empty;

    /// <summary>
    /// Translation of the surah name or description (maps to terjemah).
    /// </summary>
    public string Translation { get; set; } = string.Empty;

    /// <summary>
    /// Total number of ayah in the surah (maps to jumlahayat).
    /// </summary>
    public int TotalAyah { get; set; }

    /// <summary>
    /// Category of the surah (maps to kategori).
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Order or position of the surah (maps to posisi).
    /// </summary>
    public int Order { get; set; }
}