using QuranKemenag.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuranKemenag.Repositories
{
    /// <summary>
    /// Repository untuk mengambil data terjemahan Al-Qur'an.
    /// </summary>
    public  interface ITranslationRepository
    {
        /// <summary>
        /// Mengambil terjemahan berdasarkan nomor surah dan nomor ayat.
        /// </summary>
        Task<Translation?> GetTranslationAsync(
            int surahNumber,
            int ayatNumber);

        /// <summary>
        /// Mengambil seluruh terjemahan untuk satu surah.
        /// </summary>
        /// <param name="surahNumber">
        /// Nomor surah yang akan diambil.
        /// </param>
        /// <returns>
        /// Daftar terjemahan yang diurutkan berdasarkan nomor ayat.
        /// </returns>
        Task<IReadOnlyList<Translation>> GetTranslationsBySurahAsync(
            int surahNumber);
    }
}
