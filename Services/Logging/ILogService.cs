namespace QuranKemenag.Services.Logging;

/// <summary>
/// Menyediakan layanan logging aplikasi.
/// </summary>
public interface ILogService
{
    /// <summary>
    /// Menulis satu baris log.
    /// </summary>
    void Write(string message);

    /// <summary>
    /// Menulis pemisah log.
    /// </summary>
    void Separator();
}