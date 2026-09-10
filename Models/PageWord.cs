namespace QuranKemenag.Models;

public class PageWord
{
    public int PageNumber { get; set; }
    public int LineNumber { get; set; }
    public int WordOrder { get; set; }
    public int SurahNumber { get; set; }
    public int AyatNumber { get; set; }
    public int WordNumber { get; set; }
    public string Location { get; set; } = "";
    public string WordText { get; set; } = "";
    public string? QpcV1 { get; set; }
    public string? QpcV2 { get; set; }

    /// <summary>Teks Unicode (mode Computer Font), dari quran.kemenag.</summary>
    public string? UnicodeText { get; set; }

    /// <summary>Teks simbol/glyph (mode Handwritten), dari msi_simbol.db.</summary>
    public string? SymbolText { get; set; }

    /// <summary>
    /// Teks yang SEHARUSNYA ditampilkan untuk mode yang sedang aktif
    /// (diisi oleh MushafPageService — View tidak perlu tahu mode apa
    /// yang aktif, cukup pakai properti ini).
    /// Null jika data untuk mode ini belum tersedia di database sumber.
    /// </summary>
    public string? DisplayText { get; set; }

    /// <summary>
    /// Kode font handwritten (dari msi_simbol.db) untuk kata ini.
    /// 0/null jika mode Computer Font atau data tidak tersedia.
    /// </summary>
    public int? KodeFont { get; set; }

    /// <summary>
    /// True jika DisplayText berasal dari WordText (mushaf_layout.db)
    /// sebagai cadangan, karena data mode aktif (Unicode/Symbol) belum
    /// tersedia di database sumbernya untuk kata ini.
    /// </summary>
    public bool IsFallback { get; set; }

    /// <summary>
    /// Memecah DisplayText per token (dipisah spasi) — dipakai View
    /// untuk merender tiap glyph sebagai elemen terpisah, supaya tidak
    /// ada 2+ karakter PUA tercampur dalam 1 TextBlock yang bisa memicu
    /// salah urut oleh algoritma bidi WinUI.
    /// </summary>
    public IEnumerable<string> DisplayTextTokens =>
        (DisplayText ?? WordText)?.Split(
            ' ', StringSplitOptions.RemoveEmptyEntries)
        ?? Array.Empty<string>();
}