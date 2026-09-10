namespace QuranKemenag.Models;

public class PageLine
{
    public int PageNumber { get; set; }

    public int LineNumber { get; set; }

    public string LineType { get; set; } = "";

    public string VerseRange { get; set; } = "";

    public string LineText { get; set; } = "";

    public List<PageWord> Words { get; set; } = new();

    public List<Ayat> Ayats { get; set; } = new();
}