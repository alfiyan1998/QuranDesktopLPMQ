namespace QuranKemenag.Models;

/// <summary>
/// Data terjemahan satu ayat dari database Indonesia2019.
/// </summary>
public sealed class Translation
{
    /// <summary>
    /// ID record pada database.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nomor surah.
    /// </summary>
    public int Sura { get; set; }

    /// <summary>
    /// Nomor ayat.
    /// </summary>
    public int Aya { get; set; }

    /// <summary>
    /// Nomor juz.
    /// </summary>
    public int Juz { get; set; }

    /// <summary>
    /// Tema terjemahan.
    /// </summary>
    public string? Tema { get; set; }

    /// <summary>
    /// Teks terjemahan Bahasa Indonesia.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Nomor catatan kaki.
    /// NULL jika ayat tidak mempunyai catatan kaki.
    /// </summary>
    public string? NoFn { get; set; }

    /// <summary>
    /// Isi catatan kaki.
    /// NULL jika tidak terdapat catatan kaki.
    /// </summary>
    public string? TextFn { get; set; }
}