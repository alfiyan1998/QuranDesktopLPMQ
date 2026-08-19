using System;
using QuranInspector.Models;

namespace QuranInspector;

/// <summary>
/// Responsible for printing analysis reports to console with simple coloring.
/// </summary>
public static class ReportPrinter
{
    private static void WriteLineColor(string text, ConsoleColor color)
    {
        var prev = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = prev;
    }

    /// <summary>Print detailed analysis report.</summary>
    public static void Print(AnalysisReport report)
    {
        Console.WriteLine("\n=== Analysis Report ===\n");
        WriteLineColor("Raw Text:", ConsoleColor.Yellow);
        Console.WriteLine(report.RawText);

        Console.WriteLine();
        WriteLineColor("Characters:", ConsoleColor.Yellow);
        Console.WriteLine("Idx | Char | Code | Category | Block | RTL | Pres | PUA | Name");
        Console.WriteLine(new string('-', 120));

        foreach (var c in report.Characters)
        {
            Console.ForegroundColor = c.IsPresentationForm ? ConsoleColor.Yellow : ConsoleColor.Gray;
            Console.WriteLine($"{c.Index,3} | {c.Character} | {c.CodePointHex} | {c.Category,-20} | {c.Block,-25} | {(c.IsRtl?"RTL":"LTR"),3} | {(c.IsPresentationForm?"Y":"N"),3} | {(c.IsPrivateUse?"Y":"N"),3} | {c.Name}");
            Console.ResetColor();
        }

        Console.WriteLine();
        WriteLineColor("Statistics:", ConsoleColor.Cyan);
        Console.WriteLine($"Total characters : {report.TotalCharacters}");
        Console.WriteLine($"Arabic count      : {report.ArabicCount}");
        Console.WriteLine($"Presentation form : {report.PresentationFormCount}");
        Console.WriteLine($"Private use       : {report.PrivateUseCount}");
        Console.WriteLine($"Control chars     : {report.ControlCount}");

        Console.WriteLine();
        WriteLineColor("Diagnosis:", ConsoleColor.Green);
        if (!string.IsNullOrWhiteSpace(report.Suggestion))
        {
            WriteLineColor(report.Suggestion, ConsoleColor.Magenta);
        }
        else
        {
            WriteLineColor("No strong diagnostic. Data may already be logical Unicode.", ConsoleColor.Green);
        }
    }
}
