using System.Diagnostics;
using QuranKemenag.Constants;

namespace QuranKemenag.Services.Logging;

/// <summary>
/// Implementasi logging menggunakan Debug Window.
/// </summary>
public sealed class DebugLogService : ILogService
{
    public void Write(string message)
    {
        Debug.WriteLine(message);
    }

    public void Separator()
    {
        Debug.WriteLine(LogMessages.Separator);
    }
}