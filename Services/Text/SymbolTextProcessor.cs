using System.Diagnostics;
using QuranKemenag.Helpers;
using QuranKemenag.Models;

namespace QuranKemenag.Services.Text;

/// <summary>
/// Bertugas mengubah data simbol dari repository menjadi model Ayat
/// yang siap ditampilkan oleh UI.
/// </summary>
public sealed class SymbolTextProcessor : ISymbolTextProcessor
{
    public IReadOnlyList<Ayat> Process(IReadOnlyList<AyatSymbol> source)
    {
        var result = new List<Ayat>();

        foreach (var item in source)
        {
            string teks = SymbolTextHelper.ReverseCharacters(item.Teks);

            var ayat = new Ayat
            {
                Id = item.Id,
                Surah = item.Surah,
                AyatNumber = item.AyatNumber,
                Juz = item.Juz,
             

                Teks = teks,

                Gundul = string.Empty,
                Isyarat = string.Empty,

                // <<< INI YANG PENTING
                KodeFont = item.KodeFont,

                Halaman = item.Halaman
            };

            Debug.WriteLine(
                $"[Processor] Ayat={ayat.AyatNumber} Font={ayat.KodeFont}");

            result.Add(ayat);
        }

        return result;
    }
}