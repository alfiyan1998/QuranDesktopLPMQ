namespace QuranKemenag.Models;

/// <summary>
/// Informasi Juz Al-Qur'an.
/// </summary>
public sealed class JuzInfo
{
    /// <summary>
    /// Nomor Juz, 1 sampai 30.
    /// </summary>
    public int JuzNumber { get; set; }

    /// <summary>
    /// Nomor Surah tempat Juz dimulai.
    /// </summary>
    public int SurahNumber { get; set; }

    /// <summary>
    /// Nomor ayat tempat Juz dimulai.
    /// </summary>
    public int AyatNumber { get; set; }

    /// <summary>
    /// Nama latin surah tempat Juz dimulai.
    /// </summary>
    public string SurahName { get; set; } = string.Empty;

    /// <summary>
    /// Nama tampilan Juz.
    /// </summary>
    public string DisplayName =>
        $"Juz {JuzNumber}";

    /// <summary>
    /// Keterangan lokasi awal Juz.
    /// </summary>
    public string StartInfo =>
        $"{SurahName} • Ayat {AyatNumber}";
}