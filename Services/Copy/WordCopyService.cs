using QuranKemenag.Models.Copy;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace QuranKemenag.Services.Copy;

public sealed class WordCopyService
{
    // ============================================================
    // WINDOWS COM API
    // ============================================================

    [DllImport(
        "oleaut32.dll",
        CharSet = CharSet.Unicode,
        PreserveSig = false)]
    private static extern void GetActiveObject(
        ref Guid rclsid,
        IntPtr reserved,
        [MarshalAs(UnmanagedType.Interface)]
        out object ppunk);

    // ============================================================
    // COPY TO WORD
    // ============================================================

    public void CopyToWord(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            Debug.WriteLine(
                "[WORD COPY] Text kosong.");

            return;
        }

        object? wordApp = null;
        object? document = null;
        object? selection = null;

        try
        {
            // ====================================================
            // 1. CARI MICROSOFT WORD YANG SUDAH TERBUKA
            // ====================================================

            wordApp = GetRunningWord();

            if (wordApp != null)
            {
                Debug.WriteLine(
                    "[WORD COPY] Word sedang terbuka.");
            }
            else
            {
                // =================================================
                // 2. WORD BELUM TERBUKA
                // =================================================

                Type? wordType =
                    Type.GetTypeFromProgID(
                        "Word.Application");

                if (wordType == null)
                {
                    throw new InvalidOperationException(
                        "Microsoft Word tidak ditemukan.");
                }

                wordApp =
                    Activator.CreateInstance(
                        wordType);

                Debug.WriteLine(
                    "[WORD COPY] Word belum terbuka. Membuka Word...");
            }

            // ====================================================
            // 3. WORD VISIBLE
            // ====================================================

            dynamic app = wordApp;

            app.Visible = true;

            // ====================================================
            // 4. CEK DOCUMENT
            // ====================================================

            dynamic documents =
                app.Documents;

            if (documents.Count > 0)
            {
                document =
                    app.ActiveDocument;

                Debug.WriteLine(
                    "[WORD COPY] Menggunakan dokumen aktif.");
            }
            else
            {
                document =
                    documents.Add();

                Debug.WriteLine(
                    "[WORD COPY] Membuat dokumen baru.");
            }

            // ====================================================
            // 5. AMBIL SELECTION
            // ====================================================

            selection =
                app.Selection;

            dynamic currentSelection =
                selection;

            // ====================================================
            // 6. POSISI CURSOR DI AKHIR DOKUMEN
            // ====================================================

            currentSelection.Collapse(0);

            // ====================================================
            // 7. FORMAT DAN MASUKKAN TEXT
            // ====================================================

            InsertFormattedText(
                currentSelection,
                text);

            Debug.WriteLine(
                "[WORD COPY] Text berhasil dimasukkan ke Word.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                "[WORD COPY] ERROR:");

            Debug.WriteLine(ex);
        }
        finally
        {
            // ====================================================
            // WORD JANGAN DITUTUP
            // ====================================================

            selection = null;
            document = null;
            wordApp = null;
        }
    }

    // ============================================================
    // COPY COPYRESULT TO WORD
    // ============================================================

    public void CopyToWord(CopyResult result)
    {
        if (result == null || result.IsEmpty)
        {
            Debug.WriteLine(
                "[WORD COPY] CopyResult kosong.");

            return;
        }

        object? wordApp = null;
        object? document = null;
        object? selection = null;

        try
        {
            // ====================================================
            // 1. CARI MICROSOFT WORD YANG SUDAH TERBUKA
            // ====================================================

            wordApp = GetRunningWord();

            if (wordApp != null)
            {
                Debug.WriteLine(
                    "[WORD COPY] Word sedang terbuka.");
            }
            else
            {
                // =================================================
                // 2. WORD BELUM TERBUKA
                // =================================================

                Type? wordType =
                    Type.GetTypeFromProgID(
                        "Word.Application");

                if (wordType == null)
                {
                    throw new InvalidOperationException(
                        "Microsoft Word tidak ditemukan.");
                }

                wordApp =
                    Activator.CreateInstance(
                        wordType);

                Debug.WriteLine(
                    "[WORD COPY] Word belum terbuka. Membuka Word...");
            }

            // ====================================================
            // 3. WORD VISIBLE
            // ====================================================

            dynamic app = wordApp;

            app.Visible = true;

            // ====================================================
            // 4. CEK DOCUMENT
            // ====================================================

            dynamic documents =
                app.Documents;

            if (documents.Count > 0)
            {
                document =
                    app.ActiveDocument;

                Debug.WriteLine(
                    "[WORD COPY] Menggunakan dokumen aktif.");
            }
            else
            {
                document =
                    documents.Add();

                Debug.WriteLine(
                    "[WORD COPY] Membuat dokumen baru.");
            }

            // ====================================================
            // 5. AMBIL SELECTION
            // ====================================================

            selection =
                app.Selection;

            dynamic currentSelection =
                selection;

            // ====================================================
            // 6. POSISI CURSOR DI AKHIR DOKUMEN
            // ====================================================

            currentSelection.Collapse(0);

            // ====================================================
            // 7. INSERT COPY BLOCKS
            // ====================================================

            InsertCopyBlocks(
                currentSelection,
                result);

            Debug.WriteLine(
                "[WORD COPY] CopyResult berhasil dimasukkan ke Word.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                "[WORD COPY] ERROR:");

            Debug.WriteLine(ex);
        }
        finally
        {
            // ====================================================
            // WORD JANGAN DITUTUP
            // ====================================================

            selection = null;
            document = null;
            wordApp = null;
        }
    }

    // ============================================================
    // INSERT FORMATTED TEXT
    // ============================================================

    private static void InsertFormattedText(
        dynamic selection,
        string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        // Normalisasi line ending
        text = text.Replace(
            "\r\n",
            "\n");

        text = text.Replace(
            "\r",
            "\n");

        string[] lines =
            text.Split(
                '\n',
                StringSplitOptions.None);

        // ========================================================
        // KASUS KHUSUS
        //
        // Jika isi hanya terdiri dari Arab + referensi,
        // seluruh ayat Arab digabung menjadi satu paragraph.
        //
        // Tujuannya agar Word sendiri yang melakukan wrapping
        // sesuai lebar halaman.
        // ========================================================

        bool containsTranslation =
            ContainsTranslation(lines);

        if (!containsTranslation)
        {
            InsertArabicOnly(
                selection,
                lines);

            return;
        }

        // ========================================================
        // MODE ARAB + TERJEMAHAN
        // ========================================================

        InsertArabicAndTranslation(
            selection,
            lines);
    }

    // ============================================================
    // INSERT COPY BLOCKS
    // ============================================================

    private static void InsertCopyBlocks(
        dynamic selection,
        CopyResult result)
    {
        foreach (CopyBlock block in result.Blocks)
        {
            if (string.IsNullOrWhiteSpace(block.Text))
                continue;

            int start =
                GetSelectionStart(selection);

            selection.TypeText(
                block.Text);

            int end =
                GetSelectionStart(selection);

            switch (block.Type)
            {
                case CopyBlockType.Arabic:

                    FormatArabicBlock(
                        selection,
                        start,
                        end,
                        block);

                    break;

                case CopyBlockType.Translation:

                    FormatTranslationRange(
                        selection,
                        start,
                        end);

                    break;

                case CopyBlockType.SignLanguage:

                    FormatTranslationRange(
                        selection,
                        start,
                        end);

                    break;

                case CopyBlockType.Footnote:

                    FormatTranslationRange(
                        selection,
                        start,
                        end);

                    break;

                case CopyBlockType.SurahInfo:

                    FormatReferenceRange(
                        selection,
                        start,
                        end);

                    break;

                case CopyBlockType.Separator:

                    break;
            }

            selection.TypeParagraph();
        }
    }

    // ============================================================
    // ARAB ONLY
    // ============================================================

    private static void InsertArabicOnly(
        dynamic selection,
        string[] lines)
    {
        string arabicText = string.Empty;
        string? reference = null;

        foreach (string rawLine in lines)
        {
            string line =
                rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            // ----------------------------------------------------
            // Reference
            // ----------------------------------------------------

            if (IsReferenceLine(line))
            {
                reference = line;
                continue;
            }

            // ----------------------------------------------------
            // Arabic
            // ----------------------------------------------------

            if (ContainsArabic(line))
            {
                if (!string.IsNullOrWhiteSpace(arabicText))
                {
                    arabicText += " ";
                }

                arabicText += line;
            }
        }

        // ========================================================
        // MASUKKAN ARAB
        // ========================================================

        if (!string.IsNullOrWhiteSpace(arabicText))
        {
            int start =
                GetSelectionStart(selection);

            selection.TypeText(
                arabicText);

            int end =
                GetSelectionStart(selection);

            FormatArabicRange(
                selection,
                start,
                end);

            // Paragraph baru setelah Arab
            selection.TypeParagraph();
        }

        // ========================================================
        // REFERENCE
        // ========================================================

        if (!string.IsNullOrWhiteSpace(reference))
        {
            int start =
                GetSelectionStart(selection);

            selection.TypeText(
                reference);

            int end =
                GetSelectionStart(selection);

            FormatReferenceRange(
                selection,
                start,
                end);

            selection.TypeParagraph();
        }

        Debug.WriteLine(
            "[WORD COPY] Mode = Arabic Only");
    }

    // ============================================================
    // ARAB + TRANSLATION
    // ============================================================

    private static void InsertArabicAndTranslation(
        dynamic selection,
        string[] lines)
    {
        int i = 0;

        while (i < lines.Length)
        {
            string line =
                lines[i].Trim();

            // ----------------------------------------------------
            // Lewati baris kosong
            // ----------------------------------------------------

            if (string.IsNullOrWhiteSpace(line))
            {
                i++;
                continue;
            }

            // ----------------------------------------------------
            // Reference
            // ----------------------------------------------------

            if (IsReferenceLine(line))
            {
                InsertReference(
                    selection,
                    line);

                i++;
                continue;
            }

            // ----------------------------------------------------
            // Arabic
            // ----------------------------------------------------

            if (ContainsArabic(line))
            {
                InsertArabic(
                    selection,
                    line);

                i++;

                // ------------------------------------------------
                // Cari terjemahan setelah Arabic
                // ------------------------------------------------

                while (
                    i < lines.Length &&
                    string.IsNullOrWhiteSpace(lines[i]))
                {
                    i++;
                }

                if (i < lines.Length)
                {
                    string translation =
                        lines[i].Trim();

                    // Jika bukan Arab dan bukan reference,
                    // anggap sebagai terjemahan.
                    if (
                        !string.IsNullOrWhiteSpace(translation) &&
                        !ContainsArabic(translation) &&
                        !IsReferenceLine(translation))
                    {
                        InsertTranslation(
                            selection,
                            translation);

                        i++;
                    }
                }

                continue;
            }

            // ----------------------------------------------------
            // Teks biasa
            //
            // Misalnya catatan kaki.
            // ----------------------------------------------------

            InsertNormalText(
                selection,
                line);

            i++;
        }

        Debug.WriteLine(
            "[WORD COPY] Mode = Arabic + Translation");
    }

    // ============================================================
    // INSERT ARABIC
    // ============================================================

    private static void InsertArabic(
        dynamic selection,
        string arabic)
    {
        int start =
            GetSelectionStart(selection);

        selection.TypeText(
            arabic);

        int end =
            GetSelectionStart(selection);

        FormatArabicRange(
            selection,
            start,
            end);

        selection.TypeParagraph();
    }

    // ============================================================
    // INSERT TRANSLATION
    // ============================================================

    private static void InsertTranslation(
        dynamic selection,
        string translation)
    {
        int start =
            GetSelectionStart(selection);

        selection.TypeText(
            translation);

        int end =
            GetSelectionStart(selection);

        FormatTranslationRange(
            selection,
            start,
            end);

        selection.TypeParagraph();
    }

    // ============================================================
    // INSERT REFERENCE
    // ============================================================

    private static void InsertReference(
        dynamic selection,
        string reference)
    {
        int start =
            GetSelectionStart(selection);

        selection.TypeText(
            reference);

        int end =
            GetSelectionStart(selection);

        FormatReferenceRange(
            selection,
            start,
            end);

        selection.TypeParagraph();
    }

    // ============================================================
    // INSERT NORMAL TEXT
    // ============================================================

    private static void InsertNormalText(
        dynamic selection,
        string text)
    {
        int start =
            GetSelectionStart(selection);

        selection.TypeText(
            text);

        int end =
            GetSelectionStart(selection);

        FormatTranslationRange(
            selection,
            start,
            end);

        selection.TypeParagraph();
    }


    // ============================================================
    // FORMAT ARABIC BLOCK
    // ============================================================

    private static void FormatArabicBlock(
        dynamic selection,
        int start,
        int end,
        CopyBlock block)
    {
        if (end <= start)
            return;

        dynamic range =
            selection.Document.Range(
                start,
                end);

        // ========================================================
        // Arabic / RTL
        // ========================================================

        range.ParagraphFormat.ReadingOrder = 0;

        // ========================================================
        // Justify
        // ========================================================

        range.ParagraphFormat.Alignment = 3;

        // ========================================================
        // FONT
        // ========================================================

        if (!string.IsNullOrWhiteSpace(block.FontFamily))
        {
            range.Font.Name =
                block.FontFamily;

            try
            {
                range.Font.NameBi =
                    block.FontFamily;
            }
            catch
            {
                // Abaikan jika Word tidak menyediakan NameBi
            }
        }

        // ========================================================
        // FONT SIZE
        // ========================================================

        range.Font.Size =
            block.FontSize > 0
                ? block.FontSize
                : 18;

        // ========================================================
        // SPACING
        // ========================================================

        range.ParagraphFormat.SpaceBefore = 0;
        range.ParagraphFormat.SpaceAfter = 6;
        range.ParagraphFormat.LineSpacingRule = 0;

        range = null;
    }

    // ============================================================
    // FORMAT ARABIC
    // ============================================================

    private static void FormatArabicRange(
    dynamic selection,
    int start,
    int end)
    {
        if (end <= start)
            return;

        dynamic range =
            selection.Document.Range(
                start,
                end);

        // --------------------------------------------------------
        // Arabic / RTL
        // --------------------------------------------------------

        range.ParagraphFormat.ReadingOrder = 0;

        // --------------------------------------------------------
        // Justify
        // --------------------------------------------------------

        range.ParagraphFormat.Alignment = 3;

        // --------------------------------------------------------
        // Font
        // --------------------------------------------------------

        range.Font.Name = "LPMQ Isep Misbah";

        try
        {
            range.Font.NameBi = "LPMQ Isep Misbah";
        }
        catch
        {
            // Abaikan jika Word tidak menyediakan NameBi
        }

        range.Font.Size = 18;

        // --------------------------------------------------------
        // Spacing
        // --------------------------------------------------------

        range.ParagraphFormat.SpaceBefore = 0;
        range.ParagraphFormat.SpaceAfter = 6;
        range.ParagraphFormat.LineSpacingRule = 0;

        // --------------------------------------------------------
        // Hapus selection range
        // --------------------------------------------------------

        range = null;
    }

    // ============================================================
    // FORMAT TRANSLATION
    // ============================================================

    private static void FormatTranslationRange(
        dynamic selection,
        int start,
        int end)
    {
        if (end <= start)
            return;

        dynamic range =
            selection.Document.Range(
                start,
                end);

        // LTR
        range.ParagraphFormat.ReadingOrder = 1;

        // Left
        range.ParagraphFormat.Alignment = 0;

        range.Font.Size = 11;

        range.ParagraphFormat.SpaceBefore = 0;
        range.ParagraphFormat.SpaceAfter = 4;

        range = null;
    }

    // ============================================================
    // FORMAT REFERENCE
    // ============================================================

    private static void FormatReferenceRange(
        dynamic selection,
        int start,
        int end)
    {
        if (end <= start)
            return;

        dynamic range =
            selection.Document.Range(
                start,
                end);

        // LTR
        range.ParagraphFormat.ReadingOrder = 1;

        // Center
        range.ParagraphFormat.Alignment = 1;

        range.Font.Size = 10;

        range.ParagraphFormat.SpaceBefore = 6;
        range.ParagraphFormat.SpaceAfter = 0;

        range = null;
    }

    // ============================================================
    // GET CURRENT SELECTION POSITION
    // ============================================================

    private static int GetSelectionStart(
        dynamic selection)
    {
        return (int)selection.Start;
    }

    // ============================================================
    // CEK APAKAH ADA TERJEMAHAN
    // ============================================================

    private static bool ContainsTranslation(
        string[] lines)
    {
        foreach (string rawLine in lines)
        {
            string line =
                rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (IsReferenceLine(line))
                continue;

            if (ContainsArabic(line))
                continue;

            // Teks non-Arab berarti kemungkinan
            // terjemahan / catatan.
            return true;
        }

        return false;
    }

    // ============================================================
    // CEK ARABIC
    // ============================================================

    private static bool ContainsArabic(
        string text)
    {
        foreach (char c in text)
        {
            // Arabic
            if (c >= '\u0600' && c <= '\u06FF')
                return true;

            // Arabic Supplement
            if (c >= '\u0750' && c <= '\u077F')
                return true;

            // Arabic Extended
            if (c >= '\u08A0' && c <= '\u08FF')
                return true;
        }

        return false;
    }

    // ============================================================
    // CEK REFERENCE
    // ============================================================

    private static bool IsReferenceLine(
        string line)
    {
        string value =
            line.Trim();

        if (value.Length < 5)
            return false;

        // Contoh:
        //
        // (Al-Fatihah/1: 7)
        //
        // atau
        //
        // (Al-Baqarah/2: 5)
        //

        return
            value.StartsWith("(") &&
            value.EndsWith(")") &&
            value.Contains("/") &&
            value.Contains(":");
    }

    // ============================================================
    // GET RUNNING WORD
    // ============================================================

    private static object? GetRunningWord()
    {
        try
        {
            Guid wordGuid =
                new Guid(
                    "000209FF-0000-0000-C000-000000000046");

            GetActiveObject(
                ref wordGuid,
                IntPtr.Zero,
                out object wordObject);

            return wordObject;
        }
        catch
        {
            return null;
        }
    }
}