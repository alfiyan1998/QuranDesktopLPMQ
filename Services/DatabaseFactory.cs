using System;
using System.IO;

namespace QuranKemenag.Services;

/// <summary>
/// Resolves file system paths for the different application databases.
/// </summary>
public sealed class DatabaseFactory
{
    private readonly string _baseDirectory;

    /// <summary>
    /// Creates a new instance of <see cref="DatabaseFactory"/>.
    /// </summary>
    /// <param name="baseDirectory">Base directory where the application is running (usually AppContext.BaseDirectory).</param>
    public DatabaseFactory(string baseDirectory)
    {
        _baseDirectory = baseDirectory ?? throw new ArgumentNullException(nameof(baseDirectory));
    }

    /// <summary>
    /// Returns the full file system path for a given <see cref="DatabaseType"/>.
    /// The database files are expected to be located in a "Database" folder next to the application binary.
    /// </summary>
    /// <param name="type">The logical database identifier.</param>
    /// <returns>Full path to the requested database file.</returns>
    public string GetDatabasePath(DatabaseType type)
    {
        const string folder = "Database";
        // Map logical DatabaseType to actual file name present in the Database folder.
        // Keep filenames exact as they exist on disk (do not append/change names).
        string fileName = type switch
        {
            DatabaseType.QuranKemenag => "quran.kemenag",
            //DatabaseType.Handwritten => "quran_handwritten.db",
            // Symbol database used for rendering/metadata (msi_simbol.db)
            DatabaseType.QuranSymbol => "msi_simbol.db",
            //DatabaseType.Application => "app.db",
            DatabaseType.Translation => "Indonesia2019.db",

            DatabaseType.MushafLayout => "mushaf_layout.db",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown database type")
        };

        // Use Path.Combine to create a path that works both in development (VS) and after publishing.
        return Path.Combine(_baseDirectory, folder, fileName);
    }
}
