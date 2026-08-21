namespace QuranKemenag.Models
{
    /// <summary>
    /// Informasi footer untuk satu halaman mushaf.
    /// Menyimpan teks footer dan nomor halaman untuk render dinamis.
    /// </summary>
    public sealed class QuranPageFooter
    {
        /// <summary>
        /// Nomor halaman.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Slot teks Arab untuk footer (jika ada).
        /// </summary>
        public string FooterArabicText { get; set; } = string.Empty;
    }
}
