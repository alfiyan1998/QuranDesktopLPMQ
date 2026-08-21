namespace QuranKemenag.Models
{
    /// <summary>
    /// Posisi awal surah pada sebuah halaman.
    /// </summary>
    public enum SurahStartPosition
    {
        Start,
        Middle
    }

    /// <summary>
    /// Informasi mengenai titik pergantian/mulai sebuah surah pada halaman.
    /// </summary>
    public sealed class SurahStartInfo
    {
        public int SurahNumber { get; set; }

        public int AyatNumber { get; set; }

        public SurahStartPosition Position { get; set; }
    }
}
