using QuranKemenag.Models;

namespace QuranKemenag.Services.Text;

public interface ISymbolTextProcessor
{
    IReadOnlyList<Ayat> Process(IReadOnlyList<AyatSymbol> source);
}