using Microsoft.Data.Sqlite;
using QuranKemenag.Models;

namespace QuranKemenag.Services.Page;

public class LayoutRepository
{
    private readonly string _connectionString;

    public LayoutRepository(string databasePath)
    {
        _connectionString =
            $"Data Source={databasePath}";
    }

    public List<PageLine> GetPageLines(int pageNumber)
    {
        var lines = new List<PageLine>();

        using var connection =
            new SqliteConnection(_connectionString);

        connection.Open();

        // =========================================================
        // 1. AMBIL DATA BARIS DARI page_lines
        // =========================================================

        const string lineSql = """
            SELECT
                page_number,
                line_number,
                line_type,
                verse_range,
                line_text
            FROM page_lines
            WHERE page_number = $page
            ORDER BY line_number;
            """;

        using var lineCommand =
            new SqliteCommand(lineSql, connection);

        lineCommand.Parameters.AddWithValue(
            "$page",
            pageNumber);

        using var lineReader =
            lineCommand.ExecuteReader();

        while (lineReader.Read())
        {
            lines.Add(new PageLine
            {
                PageNumber =
                    lineReader.GetInt32(0),

                LineNumber =
                    lineReader.GetInt32(1),

                LineType =
                    lineReader.IsDBNull(2)
                        ? ""
                        : lineReader.GetString(2),

                VerseRange =
                    lineReader.IsDBNull(3)
                        ? ""
                        : lineReader.GetString(3),

                LineText =
                    lineReader.IsDBNull(4)
                        ? ""
                        : lineReader.GetString(4)
            });
        }

        // =========================================================
        // 2. AMBIL DATA KATA DARI page_words
        // =========================================================

        const string wordSql = """
            SELECT
                page_number,
                line_number,
                word_order,
                surah_number,
                ayat_number,
                word_number,
                location,
                word_text,
                qpc_v1,
                qpc_v2
            FROM page_words
            WHERE page_number = $page
            ORDER BY
                line_number,
                word_order;
            """;

        using var wordCommand =
            new SqliteCommand(wordSql, connection);

        wordCommand.Parameters.AddWithValue(
            "$page",
            pageNumber);

        using var wordReader =
            wordCommand.ExecuteReader();

        while (wordReader.Read())
        {
            var word = new PageWord
            {
                PageNumber =
                    wordReader.GetInt32(0),

                LineNumber =
                    wordReader.GetInt32(1),

                WordOrder =
                    wordReader.GetInt32(2),

                SurahNumber =
                    wordReader.GetInt32(3),

                AyatNumber =
                    wordReader.GetInt32(4),

                WordNumber =
                    wordReader.GetInt32(5),

                Location =
                    wordReader.IsDBNull(6)
                        ? ""
                        : wordReader.GetString(6),

                WordText =
                    wordReader.IsDBNull(7)
                        ? ""
                        : wordReader.GetString(7),

                QpcV1 =
                    wordReader.IsDBNull(8)
                        ? null
                        : wordReader.GetString(8),

                QpcV2 =
                    wordReader.IsDBNull(9)
                        ? null
                        : wordReader.GetString(9)
            };

            // =====================================================
            // 3. MASUKKAN WORD KE BARIS YANG SESUAI
            // =====================================================

            var line =
                lines.FirstOrDefault(
                    x =>
                        x.LineNumber ==
                        word.LineNumber);

            if (line != null)
            {
                line.Words.Add(word);
            }
        }

        return lines;
    }
}