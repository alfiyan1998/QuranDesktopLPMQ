using QuranKemenag.Models;

namespace QuranKemenag.Services;

public interface IQuranReaderFactory
{
    IQuranReaderService Create(DisplayMode mode);
}
