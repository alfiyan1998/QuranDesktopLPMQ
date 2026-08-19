// csharp
using System;

namespace QuranKemenag.Models;

/// <summary>
/// Represents a record from the symbol database (msi_simbol.db) 'quran' table.
/// This model is kept separate from <see cref="Ayat"/> because it contains
/// symbol/font related metadata used only for rendering symbols.
/// </summary>
public sealed class AyatSymbol
{
    /// <summary>
    /// Primary key identifier of the symbol record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nomor surah (1..114).
    /// </summary>
    public int Surah { get; set; }

    /// <summary>
    /// Nomor ayat di dalam surah.
    /// </summary>
    public int AyatNumber { get; set; }

    /// <summary>
    /// Nomor juz tempat ayat berada.
    /// </summary>
    public int Juz { get; set; }

    /// <summary>
    /// Teks (string) yang merepresentasikan simbol/teks untuk ayat.
    /// Diinisialisasi ke string.Empty untuk menghindari null reference.
    /// </summary>
    public string Teks { get; set; } = string.Empty;

    /// <summary>
    /// Kode font yang menunjukkan mapping simbol ke font tertentu.
    /// Diinisialisasi ke string.Empty.
    /// </summary>
    public int KodeFont { get; set; }


    public int? Halaman { get; set; }
}
