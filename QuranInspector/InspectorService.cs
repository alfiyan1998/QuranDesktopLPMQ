using System;
using System.Text;
using Microsoft.Data.Sqlite;
using QuranInspector.Models;

namespace QuranInspector;

/// <summary>
/// Service responsible for reading a SQLite database and producing analysis reports for ayat.
/// This is a lightweight, synchronous helper used by the console tool.
/// </summary>
public static class InspectorService
{
    /// <summary>
    /// Read a single ayat text from the provided SQLite file and return the raw text.
    /// </summary>
    /// <param name="dbPath">Path to SQLite database file.</param>
    /// <param name="surah">Surah number.</param>
    /// <param name="ayat">Ayat number.</param>
    /// <returns>Raw text or null if not found.</returns>
    public static string? ReadAyat(string dbPath, int surah, int ayat)
    {
        if (string.IsNullOrWhiteSpace(dbPath)) throw new ArgumentNullException(nameof(dbPath));
        var csb = new SqliteConnectionStringBuilder { DataSource = dbPath };

        using var conn = new SqliteConnection(csb.ToString());
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT teks FROM quran WHERE surah = $s AND ayat = $a LIMIT 1";
        cmd.Parameters.AddWithValue("$s", surah);
        cmd.Parameters.AddWithValue("$a", ayat);

        var result = cmd.ExecuteScalar();
        if (result is null || result == DBNull.Value) return null;
        // The column is expected to be text; cast to string directly.
        return result as string;
    }
}
