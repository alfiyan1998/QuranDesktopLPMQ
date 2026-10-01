using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using QuranKemenag.Models;
using QuranKemenag.Services;

namespace QuranKemenag.Repositories;

public sealed class BookmarkRepository
{
    private readonly DatabaseService _databaseService;

    public BookmarkRepository(DatabaseService databaseService)
    {
        _databaseService = databaseService
            ?? throw new ArgumentNullException(nameof(databaseService));
    }

    /// <summary>
    /// Menyimpan bookmark baru.
    /// </summary>
    public async Task<long> AddAsync(
        int surah,
        int ayat,
        string? note = null)
    {
        const string sql = """
            INSERT INTO bookmarks (surah, ayat, note, created_at)
            VALUES (@Surah, @Ayat, @Note, @CreatedAt);

            SELECT last_insert_rowid();
            """;

        await using var connection =
            await _databaseService
                .OpenConnectionAsync(DatabaseType.Bookmark)
                .ConfigureAwait(false);

        return await connection.ExecuteScalarAsync<long>(
            sql,
            new
            {
                Surah = surah,
                Ayat = ayat,
                Note = note,
                CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            }).ConfigureAwait(false);
    }

    /// <summary>
    /// Mengambil seluruh bookmark.
    /// </summary>
    public async Task<IReadOnlyList<Bookmark>> GetAllAsync()
    {
        const string sql = """
            SELECT
                id AS Id,
                surah AS Surah,
                ayat AS Ayat,
                note AS Note,
                created_at AS CreatedAt
            FROM bookmarks
            ORDER BY id DESC;
            """;

        await using var connection =
            await _databaseService
                .OpenConnectionAsync(DatabaseType.Bookmark)
                .ConfigureAwait(false);

        var rows = await connection
            .QueryAsync<Bookmark>(sql)
            .ConfigureAwait(false);

        return rows.ToList();
    }

    /// <summary>
    /// Mencari bookmark untuk ayat tertentu.
    /// </summary>
    public async Task<Bookmark?> GetAsync(int surah, int ayat)
    {
        const string sql = """
            SELECT
                id AS Id,
                surah AS Surah,
                ayat AS Ayat,
                note AS Note,
                created_at AS CreatedAt
            FROM bookmarks
            WHERE surah = @Surah
              AND ayat = @Ayat
            LIMIT 1;
            """;

        await using var connection =
            await _databaseService
                .OpenConnectionAsync(DatabaseType.Bookmark)
                .ConfigureAwait(false);

        return await connection.QueryFirstOrDefaultAsync<Bookmark>(
            sql,
            new
            {
                Surah = surah,
                Ayat = ayat
            }).ConfigureAwait(false);
    }

    /// <summary>
    /// Menghapus bookmark berdasarkan ID.
    /// </summary>
    public async Task DeleteAsync(long id)
    {
        const string sql = """
            DELETE FROM bookmarks
            WHERE id = @Id;
            """;

        await using var connection =
            await _databaseService
                .OpenConnectionAsync(DatabaseType.Bookmark)
                .ConfigureAwait(false);

        await connection.ExecuteAsync(
            sql,
            new { Id = id }).ConfigureAwait(false);
    }

    public async Task UpdateNoteAsync(long id, string? note)
    {
        const string sql = """
        UPDATE bookmarks
        SET note = @Note
        WHERE id = @Id;
        """;

        await using var connection =
            await _databaseService
                .OpenConnectionAsync(DatabaseType.Bookmark)
                .ConfigureAwait(false);

        await connection.ExecuteAsync(
            sql,
            new
            {
                Id = id,
                Note = note
            }).ConfigureAwait(false);
    }
}