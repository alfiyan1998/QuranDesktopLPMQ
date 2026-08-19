using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuranKemenag.Models
{
    /// <summary>
    /// Merepresentasikan satu halaman mushaf.
    /// Satu halaman dapat berisi beberapa ayat.
    /// </summary>
    public sealed class QuranPage
    {
        /// <summary>
        /// Nomor halaman mushaf.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Juz yang berkaitan dengan halaman ini.
        /// Nullable karena data halaman belum tersedia
        /// pada database saat ini.
        /// </summary>
        public int? Juz { get; set; }

        /// <summary>
        /// Kumpulan ayat yang terdapat pada halaman ini.
        /// </summary>
        public IReadOnlyList<Ayat> Ayats { get; set; }
            = new List<Ayat>();


    }
}
