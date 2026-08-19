namespace QuranKemenag.Models;

/// <summary>
/// Menyimpan informasi posisi awal suatu Juz.
/// Digunakan untuk menentukan surah, ayat, dan halaman
/// ketika pengguna memilih Juz.
/// </summary>
public sealed class JuzStartInfo
{
    /// <summary>
    /// Nomor Juz.
    /// </summary>
    public int Juz { get; init; }

    /// <summary>
    /// Nomor Surah tempat Juz dimulai.
    /// </summary>
    public int Surah { get; init; }

    /// <summary>
    /// Nomor ayat tempat Juz dimulai.
    /// </summary>
    public int Ayat { get; init; }

    /// <summary>
    /// Nomor halaman tempat Juz dimulai.
    /// </summary>
    public int Halaman { get; init; }
}