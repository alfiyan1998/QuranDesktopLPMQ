using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using QuranKemenag.Models;
using QuranKemenag.Services;

namespace QuranKemenag.Repositories;

/// <summary>
/// Repository untuk membaca database simbol (msi_simbol.db).
/// </summary>
public sealed class SymbolRepository : ISymbolRepository
{
    private readonly DatabaseService _databaseService;

    public SymbolRepository(DatabaseService databaseService)
    {
        _databaseService = databaseService
            ?? throw new ArgumentNullException(nameof(databaseService));
    }

    public async Task<IReadOnlyList<AyatSymbol>> GetAyatBySurahAsync(int surahNumber)
    {
        const string sql = @"
SELECT
    id,
    surah,
    ayat AS AyatNumber,
    juz,
    halaman AS Halaman,
    teks,
    kode_font AS KodeFont
FROM quran
WHERE surah = @SurahNumber
ORDER BY ayat;";

        await using var connection =
            await _databaseService
                .OpenConnectionAsync(DatabaseType.QuranSymbol);

        //Debug.WriteLine("--------------------------------");
        //Debug.WriteLine("DATABASE");
        //Debug.WriteLine("--------------------------------");
        //Debug.WriteLine(connection.DataSource);
        //Debug.WriteLine("--------------------------------");

        //
        // DEBUG RAW SQLITE
        //
        var rawRows = await connection.QueryAsync(@"
SELECT
    ayat,
    kode_font
FROM quran
WHERE surah=@SurahNumber
ORDER BY ayat;",
            new { SurahNumber = surahNumber });

        Debug.WriteLine("--------------------------------");
        Debug.WriteLine("RAW SQLITE");
        Debug.WriteLine("--------------------------------");

        foreach (var row in rawRows)
        {
            Debug.WriteLine(
                $"RAW => Ayat={row.ayat} kode_font={row.kode_font}");
        }

        Debug.WriteLine("--------------------------------");

        //
        // DAPPER MAPPING
        //
        var rows = await connection.QueryAsync<AyatSymbol>(
            sql,
            new { SurahNumber = surahNumber });

        var list = rows.ToList();

        Debug.WriteLine("--------------------------------");
        Debug.WriteLine("[PAGE] SymbolRepository");
        Debug.WriteLine("--------------------------------");

        foreach (var item in list)
        {
            Debug.WriteLine(
                $"[PAGE] Surah={item.Surah} " +
                $"Ayat={item.AyatNumber} " +
                $"Halaman={item.Halaman} " +
                $"Font={item.KodeFont}");
        }

        Debug.WriteLine("--------------------------------");

        return list;
    }

    public async Task<IReadOnlyList<AyatSymbol>> GetAyatByPageAsync(
    int pageNumber)
    {
        const string sql = @"
SELECT
    id,
    surah,
    ayat AS AyatNumber,
    juz,
    teks,
    kode_font AS KodeFont,
    halaman
FROM quran
WHERE halaman = @PageNumber
ORDER BY surah, ayat;";

        await using var connection =
            await _databaseService
                .OpenConnectionAsync(DatabaseType.QuranSymbol)
                .ConfigureAwait(false);

        var rows = await connection
            .QueryAsync<AyatSymbol>(
                sql,
                new { PageNumber = pageNumber })
            .ConfigureAwait(false);

        return rows.ToList();
    }

    /// <summary>
    /// Mengambil seluruh data simbol untuk satu Juz LANGSUNG dari
    /// msi_simbol.db (kolom 'juz' pada database ini), tanpa melibatkan
    /// database lain sama sekali.
    ///
    /// Jika msi_simbol.db belum memiliki data untuk sebagian/seluruh
    /// ayat pada Juz tersebut, ayat itu TIDAK akan muncul di hasil ini.
    /// Ini disengaja: mode Handwritten hanya boleh menampilkan apa yang
    /// benar-benar ada di msi_simbol.db.
    /// </summary>
    public async Task<IReadOnlyList<AyatSymbol>> GetAyatByJuzAsync(
    int juzNumber)
    {
        const string sql = @"
SELECT
    id,
    surah,
    ayat AS AyatNumber,
    juz,
    halaman AS Halaman,
    teks,
    kode_font AS KodeFont
FROM quran
WHERE juz = @JuzNumber
ORDER BY surah, ayat;";

        await using var connection =
            await _databaseService
                .OpenConnectionAsync(DatabaseType.QuranSymbol)
                .ConfigureAwait(false);

        var rows = await connection
            .QueryAsync<AyatSymbol>(
                sql,
                new { JuzNumber = juzNumber })
            .ConfigureAwait(false);

        var list = rows.ToList();

        Debug.WriteLine(
            $"[JUZ SYMBOL] Juz={juzNumber} AyatCount={list.Count}");

        return list;
    }


    public async Task<AyatSymbol?> GetAyatAsync(int surahNumber, int ayatNumber)
    {
        const string sql = @"
SELECT
    id,
    surah,
    ayat AS AyatNumber,
    juz,
    halaman AS Halaman,
    teks,
    kode_font AS KodeFont
FROM quran
WHERE surah = @SurahNumber AND ayat = @AyatNumber
LIMIT 1;";

        await using var connection =
            await _databaseService
                .OpenConnectionAsync(DatabaseType.QuranSymbol)
                .ConfigureAwait(false);

        return await connection
            .QueryFirstOrDefaultAsync<AyatSymbol>(
                sql,
                new { SurahNumber = surahNumber, AyatNumber = ayatNumber })
            .ConfigureAwait(false);
    }

    public async Task<int> GetTotalPagesAsync()
    {
        const string sql = @"
SELECT COALESCE(MAX(halaman), 0)
FROM quran
WHERE halaman IS NOT NULL
  AND halaman > 0;";

        await using var connection =
            await _databaseService
                .OpenConnectionAsync(DatabaseType.QuranSymbol)
                .ConfigureAwait(false);

        return await connection
            .ExecuteScalarAsync<int>(sql)
            .ConfigureAwait(false);
    }
}