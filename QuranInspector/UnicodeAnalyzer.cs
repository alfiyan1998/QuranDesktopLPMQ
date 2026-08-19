using System;
using System.Globalization;
using System.Text;
using QuranInspector.Models;

namespace QuranInspector;

/// <summary>
/// Utility class that analyzes Unicode runes and classifies blocks, direction, and presentation.
/// </summary>
public static class UnicodeAnalyzer
{
    /// <summary>
    /// Analyze a single rune (code point) and return CharacterInfo.
    /// </summary>
    public static CharacterInfo AnalyzeRune(System.Text.Rune rune, int index)
    {
        int cp = rune.Value;
        string ch = rune.ToString();
        var cat = CharUnicodeInfo.GetUnicodeCategory(ch, 0);
        var block = GetUnicodeBlock(cp);
        bool isRtl = IsProbablyRtl(cp);
        bool isPresentation = (cp >= 0xFB50 && cp <= 0xFDFF) || (cp >= 0xFE70 && cp <= 0xFEFF);
        bool isPua = (cp >= 0xE000 && cp <= 0xF8FF) || (cp >= 0xF0000 && cp <= 0xFFFFD) || (cp >= 0x100000 && cp <= 0x10FFFD);
        // Getting the formal Unicode character name may require APIs not available
        // on all runtimes. For debugging purposes we leave the name empty and focus
        // on code point, category and block which are sufficient for diagnosis.
        string name = string.Empty;

        return new CharacterInfo
        {
            Index = index,
            Character = ch,
            CodePoint = cp,
            Category = cat,
            Block = block,
            IsRtl = isRtl,
            IsPresentationForm = isPresentation,
            IsPrivateUse = isPua,
            Name = name
        };
    }

    /// <summary>Simple heuristic whether codepoint is in Arabic-related RTL ranges.</summary>
    public static bool IsProbablyRtl(int cp)
    {
        if ((cp >= 0x0600 && cp <= 0x06FF) || (cp >= 0x0750 && cp <= 0x077F) || (cp >= 0x08A0 && cp <= 0x08FF)) return true;
        if ((cp >= 0xFB50 && cp <= 0xFDFF) || (cp >= 0xFE70 && cp <= 0xFEFF)) return true;
        return false;
    }

    /// <summary>Determine a simple block name for Arabic-related ranges and others.</summary>
    public static string GetUnicodeBlock(int cp)
    {
        if (cp >= 0x0600 && cp <= 0x06FF) return "Arabic";
        if (cp >= 0x0750 && cp <= 0x077F) return "Arabic Supplement";
        if (cp >= 0x08A0 && cp <= 0x08FF) return "Arabic Extended-A";
        if (cp >= 0xFB50 && cp <= 0xFDFF) return "Arabic Presentation Forms-A";
        if (cp >= 0xFE70 && cp <= 0xFEFF) return "Arabic Presentation Forms-B";
        if (cp >= 0xE000 && cp <= 0xF8FF) return "Private Use Area (BMP)";
        if (cp >= 0xF0000 && cp <= 0xFFFFD) return "Private Use Area (SMP)";
        if (cp >= 0x100000 && cp <= 0x10FFFD) return "Private Use Area (SMP2)";
        if (cp >= 0x0000 && cp <= 0x007F) return "Basic Latin";
        return "Other";
    }
}
