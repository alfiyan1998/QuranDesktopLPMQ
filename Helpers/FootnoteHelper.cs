using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuranKemenag.Helpers
{
    public static class FootnoteHelper
    {

        /// <summary>
        /// Mengubah string nomor footnote menjadi daftar nomor.
        ///
        /// Contoh:
        /// "4"        -> [4]
        /// "9,10,11"  -> [9,10,11]
        /// null       -> []
        /// ""         -> []
        /// </summary>
        public static IReadOnlyList<int> ParseNumbers(
            string? noFn)
        {
            if (string.IsNullOrWhiteSpace(noFn))
            {
                return Array.Empty<int>();
            }

            return noFn
                .Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .ToList();
        }

        /// <summary>
        /// Memeriksa apakah ayat mempunyai catatan kaki.
        /// </summary>
        public static bool HasFootnote(
            string? noFn,
            string? textFn)
        {
            return
                ParseNumbers(noFn).Count > 0 &&
                !string.IsNullOrWhiteSpace(textFn);
        }
    }
}
