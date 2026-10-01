namespace QuranKemenag.Models
{
    public sealed class Bookmark
    {
        public long Id { get; set; }

        public int Surah { get; set; }

        public int Ayat { get; set; }

        public string? Note { get; set; }

        public bool HasNote =>
                !string.IsNullOrWhiteSpace(Note);

        public string CreatedAt { get; set; } = string.Empty;

        public string SurahName { get; set; } = string.Empty;

        public string DisplayText =>
            $"{SurahName} • Ayat {Ayat}";
    }
}
