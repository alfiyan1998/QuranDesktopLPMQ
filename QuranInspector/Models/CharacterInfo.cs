using System.Globalization;

namespace QuranInspector.Models;

/// <summary>
/// Represents information about a single Unicode character (rune) in a text.
/// </summary>
public sealed class CharacterInfo
{
    /// <summary>Zero-based index of the rune in the string (rune index, not UTF-16 index).</summary>
    public int Index { get; init; }

    /// <summary>The character as string (may be surrogate pair).</summary>
    public string Character { get; init; } = string.Empty;

    /// <summary>Unicode code point value, e.g. 0x0627.</summary>
    public int CodePoint { get; init; }

    /// <summary>Formatted code point like U+0627.</summary>
    public string CodePointHex => $"U+{CodePoint:X4}";

    /// <summary>Unicode category (Letter, Mark, Punctuation, etc.).</summary>
    public UnicodeCategory Category { get; init; }

    /// <summary>Unicode block name (Arabic, Presentation Forms, etc.).</summary>
    public string Block { get; init; } = string.Empty;

    /// <summary>Whether the rune is likely RTL.</summary>
    public bool IsRtl { get; init; }

    /// <summary>Whether the rune belongs to presentation forms ranges.</summary>
    public bool IsPresentationForm { get; init; }

    /// <summary>Whether the rune is inside Private Use Area ranges.</summary>
    public bool IsPrivateUse { get; init; }

    /// <summary>Unicode character name (if available).</summary>
    public string Name { get; init; } = string.Empty;
}
