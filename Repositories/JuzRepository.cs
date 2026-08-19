using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using QuranKemenag.Models;
using QuranKemenag.Services;

namespace QuranKemenag.Repositories;

/// <summary>
/// Repository khusus untuk navigasi Juz.
/// Tidak mengubah IQuranRepository agar repository
/// Unicode/Symbol/Handwritten yang sudah ada tetap aman.
/// </summary>
public sealed class JuzRepository
{
    private readonly DatabaseService _databaseService;

    public JuzRepository(DatabaseService databaseService)
    {
        _databaseService =
            databaseService ?? throw new ArgumentNullException(nameof(databaseService));
    }

    /// <summary>
    /// Membuat daftar Juz 1 sampai 30.
    /// </summary>
    public IReadOnlyList<int> GetAllJuz()
    {
        return Enumerable.Range(1, 30).ToList();
    }

    /// <summary>
    /// Mengambil ayat pertama dari sebuah Juz.
    /// Data diambil langsung dari tabel quran.
    /// </summary>
    public async Task<JuzInfo?> GetJuzStartAsync(int juzNumber)
    {
        const string sql = """
            SELECT
                q.juz AS JuzNumber,
                q.surah AS SurahNumber,
                q.ayat AS AyatNumber,
                COALESCE(n.namalatin, '') AS SurahName
            FROM quran q
            LEFT JOIN namasurah n
                ON n.id = q.surah
            WHERE q.juz = @JuzNumber
            ORDER BY q.surah ASC, q.ayat ASC
            LIMIT 1;
            """;

        await using var connection =
            await _databaseService
                .OpenConnectionAsync(DatabaseType.QuranKemenag)
                .ConfigureAwait(false);

        return await connection
            .QueryFirstOrDefaultAsync<JuzInfo>(
                sql,
                new
                {
                    JuzNumber = juzNumber
                })
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Mengambil seluruh informasi awal Juz 1 sampai 30.
    /// </summary>
    public async Task<IReadOnlyList<JuzInfo>> GetAllJuzInfoAsync()
    {
        const string sql = """
            SELECT
                q.juz AS JuzNumber,
                q.surah AS SurahNumber,
                q.ayat AS AyatNumber,
                COALESCE(n.namalatin, '') AS SurahName
            FROM quran q
            LEFT JOIN namasurah n
                ON n.id = q.surah
            WHERE q.juz BETWEEN 1 AND 30
              AND q.ayat IS NOT NULL
            GROUP BY q.juz
            ORDER BY q.juz ASC;
            """;

        await using var connection =
            await _databaseService
                .OpenConnectionAsync(DatabaseType.QuranKemenag)
                .ConfigureAwait(false);

        var result =
            await connection
                .QueryAsync<JuzInfo>(sql)
                .ConfigureAwait(false);

        return result.ToList();
    }
}