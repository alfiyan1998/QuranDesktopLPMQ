using System;
using System.Linq;
using System.Text;
using QuranInspector.Models;
using QuranInspector;

/*
  QuranInspector - simple console tool to inspect a single ayat from a SQLite database
  Usage:
    dotnet run --project QuranInspector -- <path-to-db> <surah> <ayat>

  The tool prints each character's code point, Unicode category, block, and diagnostics.
*/

namespace QuranInspector
{
    class Program
    {
        static int Main(string[] args)
        {
        Console.OutputEncoding = Encoding.UTF8;

        if (args.Length < 3)
        {
            Console.WriteLine("Usage: QuranInspector <path-to-db> <surah> <ayat>");
            return 1;
        }

        var dbPath = args[0];
        if (!int.TryParse(args[1], out int surah))
        {
            Console.WriteLine("Invalid surah number.");
            return 1;
        }
        if (!int.TryParse(args[2], out int ayat))
        {
            Console.WriteLine("Invalid ayat number.");
            return 1;
        }

        try
        {
            var raw = InspectorService.ReadAyat(dbPath, surah, ayat);
            if (raw is null)
            {
                Console.WriteLine("Ayat not found in database.");
                return 1;
            }

            var report = new AnalysisReport { RawText = raw };

            int idx = 0;
            foreach (var rune in raw.EnumerateRunes())
            {
                var ci = UnicodeAnalyzer.AnalyzeRune(rune, idx);
                report.Characters.Add(ci);
                idx++;
            }

            // Simple heuristics
            string suggestion = GenerateSuggestion(report);
            report.Suggestion = suggestion;

            ReportPrinter.Print(report);

            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: " + ex);
            Console.ResetColor();
            return 2;
        }
    }

    // Basic automatic analysis heuristics to suggest what encoding might be used
    private static string GenerateSuggestion(AnalysisReport report)
    {
        if (report.TotalCharacters == 0) return "Empty text.";

        double presRatio = (double)report.PresentationFormCount / report.TotalCharacters;
        double puaRatio = (double)report.PrivateUseCount / Math.Max(1, report.TotalCharacters);

        if (puaRatio > 0.05)
        {
            return "Terdeteksi sejumlah karakter di Private Use Area (PUA). Kemungkinan encoding font-specific / legacy.";
        }

        if (presRatio > 0.05)
        {
            return "Terdeteksi Presentation Forms. Kemungkinan data menggunakan presentation-form glyphs (compatibility forms) atau visual-order.";
        }

        if (report.PresentationFormCount == 0 && report.PrivateUseCount == 0 && report.ArabicCount > 0)
        {
            return "Database kemungkinan menggunakan Unicode Arabic biasa (logical order).";
        }

        // fallback heuristic: if ArabicCount is small relative to total, might be mixed
        return "Hasil tidak tegas. Periksa sample lebih banyak atau jalankan pipeline deteksi visual-order lebih lanjut.";
        }
    }
}
