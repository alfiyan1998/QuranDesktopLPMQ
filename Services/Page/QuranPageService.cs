using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuranKemenag.Models;

namespace QuranKemenag.Services.Page
{
    public sealed class QuranPageService : IQuranPageService
    {
        /// <summary>
        /// Mengelompokkan ayat berdasarkan nomor halaman.
        /// 
        /// Ayat yang belum mempunyai halaman akan diabaikan.
        /// Ini diperlukan agar database lama yang belum memiliki
        /// data halaman tetap dapat digunakan.
        /// </summary>
        public IReadOnlyList<QuranPage> BuildPages(
            IReadOnlyList<Ayat> ayats)
        {
            if (ayats is null ||
            ayats.Count == 0)
            {
                return Array.Empty<QuranPage>();
            }

           var pages =
           ayats
               .Where(x => x.Halaman.HasValue)
               .GroupBy(x => x.Halaman!.Value)
               .OrderBy(x => x.Key)
               .Select(group =>
                   new QuranPage
                   {
                       PageNumber = group.Key,

                       Juz = group
                           .Select(x => (int?)x.Juz)
                           .FirstOrDefault(),

                       Ayats = group
                           .OrderBy(x => x.Surah)
                           .ThenBy(x => x.AyatNumber)
                           .ToList()
                   })
               .ToList();

            return pages;
        }

        /// <summary>
        /// Mengambil halaman berdasarkan nomor halaman.
        /// </summary>
        public QuranPage? GetPage(
            IReadOnlyList<QuranPage> pages,
            int pageNumber)
        {
            if (pages is null ||
           pages.Count == 0)
            {
                return null;
            }

            return pages.FirstOrDefault(
                x => x.PageNumber == pageNumber);
        }
    }
}
