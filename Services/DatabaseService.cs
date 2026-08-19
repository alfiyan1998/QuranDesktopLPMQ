using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace QuranKemenag.Services;

/// <summary>
/// Service responsible for opening/closing SQLite connections and validating database files.
/// </summary>
public sealed class DatabaseService
{
    private readonly DatabaseFactory _factory;

    /// <summary>
    /// Creates a new instance of <see cref="DatabaseService"/>.
    /// </summary>
    /// <param name="factory">Factory used to resolve database file paths.</param>
    public DatabaseService(DatabaseFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>
    /// Creates a new <see cref="SqliteConnection"/> for the specified database type.
    /// The connection is NOT opened by this method.
    ///
    /// Supports all <see cref="DatabaseType"/> values including <see cref="DatabaseType.QuranSymbol"/>.
    /// </summary>
    /// <param name="type">The database to create a connection for.</param>
    /// <returns>A new <see cref="SqliteConnection"/> instance.</returns>
    public SqliteConnection CreateConnection(DatabaseType type)
    {
        string path = GetDatabasePath(type);
        // Use standard connection string. Mode can be adjusted if needed.
        var connectionString = new SqliteConnectionStringBuilder { DataSource = path }.ToString();
        return new SqliteConnection(connectionString);
    }

    /// <summary>
    /// Opens an asynchronous connection to the specified database.
    /// Caller is responsible for closing/disposing the returned connection.
    ///
    /// This method supports opening the primary Quran database as well as the
    /// secondary symbol database (<see cref="DatabaseType.QuranSymbol"/>). It will
    /// validate that the underlying file exists before attempting to open.
    /// </summary>
    /// <param name="type">The database to open.</param>
    /// <returns>An opened <see cref="SqliteConnection"/>.</returns>
    public async Task<SqliteConnection> OpenConnectionAsync(DatabaseType type)
    {
        string path = GetDatabasePath(type);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Database file not found: {path}", path);
        }

        try
        {
            var connection = CreateConnection(type);
            await connection.OpenAsync().ConfigureAwait(false);
            return connection;
        }
        catch (Exception ex)
        {
            // Wrap exceptions so callers get a clear context that opening the database failed.
            throw new InvalidOperationException($"Failed to open database '{type}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Closes and disposes the provided <see cref="SqliteConnection"/> asynchronously.
    /// </summary>
    /// <param name="connection">Connection to close. If null, method does nothing.</param>
    public static async Task CloseConnectionAsync(SqliteConnection? connection)
    {
        if (connection is null)
        {
            return;
        }

        try
        {
            await connection.CloseAsync().ConfigureAwait(false);
        }
        catch
        {
            // Swallow exceptions during close to avoid hiding earlier errors. Ensure disposal.
        }
        finally
        {
            connection.Dispose();
        }
    }

    /// <summary>
    /// Checks whether the specified database file exists on disk.
    ///
    /// This is a thin helper that delegates to <see cref="DatabaseFactory"/> so
    /// callers can ask about availability of any supported DatabaseType (including
    /// <see cref="DatabaseType.QuranSymbol"/>).
    /// </summary>
    /// <param name="type">Database type to check.</param>
    /// <returns>True if file exists; otherwise false.</returns>
    public bool DatabaseExists(DatabaseType type)
    {
        string path = GetDatabasePath(type);
        return File.Exists(path);
    }

    /// <summary>
    /// Returns the full file system path for the specified database type.
    /// </summary>
    /// <param name="type">Database type.</param>
    /// <returns>Full path to database file.</returns>
    public string GetDatabasePath(DatabaseType type)
        => _factory.GetDatabasePath(type);
}
