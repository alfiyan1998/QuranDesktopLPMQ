using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using QuranKemenag.Models;
using QuranKemenag.Services;

namespace QuranKemenag.Repositories
{
    /// <summary>
    /// Repository untuk membaca database terjemahan Indonesia2019.
    /// </summary>
    public sealed class TranslationRepository : ITranslationRepository
    {
        private readonly DatabaseService _databaseService;

        /// <summary>
        /// Membuat instance TranslationRepository.
        /// </summary>
        /// <param name="databaseService">
        /// Service untuk membuka koneksi database.
        /// </param>

        public TranslationRepository(DatabaseService databaseService)
        {

            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        }

        /// <summary>
        /// Mengambil terjemahan berdasarkan nomor surah dan nomor ayat.
        /// </summary>
        public async Task<Translation?> GetTranslationAsync(
            int surahNumber,
            int ayatNumber)
        {
            const string sql = """
            SELECT
                id AS Id,
                sura AS Sura,
                aya AS Aya,
                juz AS Juz,
                tema AS Tema,
                text AS Text,
                NULLIF(no_fn, '') AS NoFn,
                text_fn AS TextFn
            FROM Indonesia2019
            WHERE sura = @SurahNumber
              AND aya = @AyatNumber
            LIMIT 1;
            """;

            await using var connection =
                await _databaseService
                    .OpenConnectionAsync(DatabaseType.Translation)
                    .ConfigureAwait(false);

            return await connection
                .QueryFirstOrDefaultAsync<Translation>(
                    sql,
                    new
                    {
                        SurahNumber = surahNumber,
                        AyatNumber = ayatNumber
                    })
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Mengambil seluruh terjemahan dalam satu surah.
        /// </summary>
        public async Task<IReadOnlyList<Translation>>
            GetTranslationsBySurahAsync(int surahNumber)
        {
            const string sql = """
            SELECT
                id AS Id,
                sura AS Sura,
                aya AS Aya,
                juz AS Juz,
                tema AS Tema,
                text AS Text,
                NULLIF(no_fn, '') AS NoFn,
                text_fn AS TextFn
            FROM Indonesia2019
            WHERE sura = @SurahNumber
            ORDER BY aya ASC;
            """;

            await using var connection =
                await _databaseService
                    .OpenConnectionAsync(DatabaseType.Translation)
                    .ConfigureAwait(false);

            var rows =
                await connection
                    .QueryAsync<Translation>(
                        sql,
                        new
                        {
                            SurahNumber = surahNumber
                        })
                    .ConfigureAwait(false);

            return rows.ToList();
        }

    }
}
