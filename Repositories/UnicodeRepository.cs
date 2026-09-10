// csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using QuranKemenag.Models;
using QuranKemenag.Services;
using System.Diagnostics;

namespace QuranKemenag.Repositories;

/// <summary>
/// Implementasi <see cref="IQuranRepository"/> yang menggunakan DatabaseService + Dapper
/// untuk mengambil daftar surah dari tabel namasurah.
/// </summary>
public sealed class UnicodeRepository : IQuranRepository
{
    private readonly DatabaseService _databaseService;

    /// <summary>
    /// Membuat instance <see cref="UnicodeRepository"/>.
    /// </summary>
    /// <param name="databaseService">Service untuk membuka koneksi ke database.</param>
    public UnicodeRepository(DatabaseService databaseService)
    {
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Surah>> GetAllSurahAsync()
    {
        const string sql = @"SELECT
    id AS Id,
    namalatin AS LatinName,
    namaarab AS ArabicName,
    jumlahayat AS TotalAyah,
    kategory AS Category,
    terjemah AS Translation,
    posisi AS [Order]
FROM namasurah
ORDER BY id;";

        try
        {
            await using var connection = await _databaseService.OpenConnectionAsync(DatabaseType.QuranKemenag).ConfigureAwait(false);
            var result = await connection.QueryAsync<Surah>(sql).ConfigureAwait(false);
            return result.ToList();
        }
        catch
        {
            throw;
        }

    }

    /// <summary>
    /// Mengambil daftar ayat untuk surah tertentu dari tabel 'quran' di database.
    /// Implementasi ini menggunakan Dapper untuk memetakan kolom SQL ke properti model <see cref="Models.Ayat"/>.
    /// Gunakan parameterisasi Dapper untuk mencegah SQL injection dan pastikan pengurutan berdasarkan nomor ayat.
    /// </summary>
    /// <param name="surahNumber">Nomor surah (contoh: 1 untuk Al-Fatihah).</param>
    /// <returns>Read-only list dari <see cref="Ayat"/>.</returns>
    public async Task<IReadOnlyList<Ayat>> GetAyatBySurahAsync(int surahNumber)
    {
        // SQL berikut membaca dari tabel 'quran' dan meng-ALIAS kolom 'ayat' menjadi
        // 'AyatNumber' agar cocok dengan properti pada model Ayat.
        // Kolom lain sudah memiliki nama yang sama dengan properti model sehingga tidak
        // memerlukan alias tambahan.
        const string sql = @"SELECT
                    id,
                    surah,
                    ayat AS AyatNumber,
                    juz,
                    halaman AS Halaman,
                    teks,
                    gundul,
                    isyarat
                FROM quran
                WHERE surah = @SurahNumber
                ORDER BY ayat ASC;";

        try
        {
            // Buka koneksi ke database menggunakan DatabaseService yang sudah tersedia.
            await using var connection = await _databaseService.OpenConnectionAsync(DatabaseType.QuranKemenag).ConfigureAwait(false);

            // Eksekusi query secara asynchronous menggunakan Dapper dan parameterisasi.
            // Gunakan anonymous object untuk memberikan parameter ke Dapper.
            var rows = await connection.QueryAsync<Ayat>(sql, new { SurahNumber = surahNumber }).ConfigureAwait(false);

            var list = rows.ToList();

            Debug.WriteLine("--------------------------------");
            Debug.WriteLine("[PAGE] UnicodeRepository");
            Debug.WriteLine("--------------------------------");

            foreach (var ayat in list)
            {
                Debug.WriteLine(
                    $"[PAGE] Surah={ayat.Surah} " +
                    $"Ayat={ayat.AyatNumber} " +
                    $"Halaman={ayat.Halaman}");
            }

            Debug.WriteLine("--------------------------------");

            return list;

            // Kembalikan sebagai IReadOnlyList agar caller tidak dapat memodifikasi koleksi secara langsung.
            //return rows.ToList();
        }
        catch
        {
            // Biarkan exception mengalir ke caller setelah dicatat/debug pada lapisan atas.
            throw;
        }
    }

    /// <summary>
    /// Mengambil seluruh ayat berdasarkan nomor halaman.
    /// </summary>
    public async Task<IReadOnlyList<Ayat>> GetAyatByPageAsync(
        int pageNumber)
    {
        const string sql = @"
        SELECT
            id,
            surah,
            ayat AS AyatNumber,
            juz,
            teks,
            gundul,
            isyarat,
            halaman AS Halaman
        FROM quran
        WHERE halaman = @PageNumber
        ORDER BY surah ASC, ayat ASC;";

        await using var connection =
            await _databaseService
                .OpenConnectionAsync(
                    DatabaseType.QuranKemenag);

        var result =
            await connection.QueryAsync<Ayat>(
                sql,
                new
                {
                    PageNumber = pageNumber
                });

        return result.ToList();
    }


    /// <summary>
    /// Mengambil jumlah halaman Quran.
    /// </summary>
    public async Task<int> GetTotalPageAsync()
    {
        const string sql = @"
        SELECT MAX(halaman)
        FROM quran
        WHERE halaman IS NOT NULL;";

        await using var connection =
            await _databaseService
                .OpenConnectionAsync(
                    DatabaseType.QuranKemenag);

        var result =
            await connection.ExecuteScalarAsync<int?>(
                sql);

        return result ?? 0;
    }

    /// <summary>
    /// Mengambil posisi ayat pertama pada suatu Juz.
    /// Data diambil berdasarkan nomor Juz terkecil,
    /// kemudian Surah dan Ayat terkecil.
    /// </summary>
    /// <param name="juzNumber">Nomor Juz 1-30.</param>
    /// <returns>
    /// Informasi posisi awal Juz, atau null jika tidak ditemukan.
    /// </returns>
    public async Task<JuzStartInfo?> GetJuzStartAsync(int juzNumber)
    {
        const string sql = @"
                SELECT
                    juz AS Juz,
                    surah AS Surah,
                    ayat AS Ayat,
                    COALESCE(halaman, 0) AS Halaman
                FROM quran
                WHERE juz = @JuzNumber
                ORDER BY surah ASC, ayat ASC
                LIMIT 1;";

        await using var connection =
            await _databaseService
                .OpenConnectionAsync(
                    DatabaseType.QuranKemenag)
                .ConfigureAwait(false);

        return await connection
            .QueryFirstOrDefaultAsync<JuzStartInfo>(
                sql,
                new
                {
                    JuzNumber = juzNumber
                })
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Ayat>> GetAyatByJuzAsync(
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
    gundul,
    isyarat
FROM quran
WHERE juz = @JuzNumber
ORDER BY surah ASC, ayat ASC;";

        await using var connection =
            await _databaseService
                .OpenConnectionAsync(
                    DatabaseType.QuranKemenag)
                .ConfigureAwait(false);

        var result =
            await connection.QueryAsync<Ayat>(
                sql,
                new
                {
                    JuzNumber = juzNumber
                })
            .ConfigureAwait(false);

        var list =
            result.ToList();

        Debug.WriteLine(
            $"[JUZ REPOSITORY] " +
            $"Juz={juzNumber} " +
            $"AyatCount={list.Count}");

        return list;
    }

    public async Task<Ayat?> GetAyatAsync(
    int surahNumber,
    int ayatNumber)
    {
        const string sql = @"
        SELECT
            id,
            surah,
            ayat AS AyatNumber,
            juz,
            halaman AS Halaman,
            teks,
            gundul,
            isyarat
        FROM quran
        WHERE surah = @SurahNumber
          AND ayat = @AyatNumber
        LIMIT 1;";

        await using var connection =
            await _databaseService
                .OpenConnectionAsync(
                    DatabaseType.QuranKemenag)
                .ConfigureAwait(false);

        return await connection
            .QueryFirstOrDefaultAsync<Ayat>(
                sql,
                new
                {
                    SurahNumber = surahNumber,
                    AyatNumber = ayatNumber
                })
            .ConfigureAwait(false);
    }
}
