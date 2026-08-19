using System.Collections.Generic;

namespace QuranInspector.Models;

/// <summary>
/// Represents analysis result for a single ayat.
/// </summary>
public sealed class AnalysisReport
{
    /// <summary>Raw text retrieved from database.</summary>
    public string RawText { get; init; } = string.Empty;

    /// <summary>List of per-character information.</summary>
    public List<CharacterInfo> Characters { get; } = new();

    /// <summary>Total count of characters (runes).</summary>
    public int TotalCharacters => Characters.Count;

    /// <summary>Count of characters in Arabic blocks.</summary>
    public int ArabicCount => Characters.Count(c => c.Block == "Arabic" || c.Block == "Arabic Supplement" || c.Block == "Arabic Extended-A");

    /// <summary>Count of presentation forms.</summary>
    public int PresentationFormCount => Characters.Count(c => c.IsPresentationForm);

    /// <summary>Count of private-use characters.</summary>
    public int PrivateUseCount => Characters.Count(c => c.IsPrivateUse);

    /// <summary>Count of control characters.</summary>
    public int ControlCount => Characters.Count(c => c.Category == System.Globalization.UnicodeCategory.Control);

    /// <summary>Suggested diagnosis from analyzer.</summary>
    public string Suggestion { get; set; } = string.Empty;
}
