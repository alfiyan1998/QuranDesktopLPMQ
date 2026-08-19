using QuranKemenag.Models;

namespace QuranKemenag.Services;

/// <summary>
/// Factory untuk memilih ReaderService sesuai mode tampilan.
/// </summary>
public sealed class ReaderServiceFactory
{
    //private readonly ComputerFontReaderService _computerReader;
    //private readonly HandwrittenManuscriptReaderService _symbolReader;

    private readonly IQuranReaderService _computerReader;
    private readonly IQuranReaderService _symbolReader;

    public ReaderServiceFactory(
    ComputerFontReaderService computerReader,
    HandwrittenManuscriptReaderService symbolReader)
    {
        _computerReader = computerReader;
        _symbolReader = symbolReader;
    }

    /// <summary>
    /// Mengembalikan ReaderService sesuai mode pembacaan.
    /// </summary>
    public IQuranReaderService Create(ReaderMode mode)
    {
        return mode switch
        {
            ReaderMode.Symbol => _symbolReader,

            ReaderMode.Unicode => _computerReader,

            _ => throw new NotSupportedException(
                    $"ReaderMode '{mode}' belum didukung.")
        };
    }
}