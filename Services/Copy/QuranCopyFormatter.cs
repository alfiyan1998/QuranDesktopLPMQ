using QuranKemenag.Helpers;
using QuranKemenag.Models;
using QuranKemenag.Models.Copy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuranKemenag.Services.Copy;

/// <summary>
/// Mengubah kumpulan ayat menjadi teks siap copy.
///
/// Formatter TIDAK mengetahui database.
/// Formatter hanya menerima data Ayat + CopyOptions
/// kemudian menghasilkan string.
/// </summary>
public sealed class QuranCopyFormatter
{
    private const string OpenParenthesis = "﴿";
    private const string CloseParenthesis = "﴾";

    /// <summary>
    /// Format seluruh ayat menjadi satu string.
    /// </summary>
    public string Format(
        IReadOnlyList<Ayat> ayats,
        CopyOptions options,
        IReadOnlyDictionary<int, string>? surahNames = null)
    {
        if (ayats is null || ayats.Count == 0)
            return string.Empty;

        if (options is null)
            throw new ArgumentNullException(nameof(options));

        var output = new StringBuilder();

        int? previousSurah = null;

        foreach (var ayat in ayats)
        {
            // -----------------------------------------------------
            // Jika berganti surah
            // -----------------------------------------------------

            bool isNewSurah =
                previousSurah.HasValue &&
                previousSurah.Value != ayat.Surah;

            if (isNewSurah)
            {
                AppendNewSurahSeparator(
                    output,
                    ayat,
                    options,
                    surahNames);
            }

            // -----------------------------------------------------
            // Format satu ayat
            // -----------------------------------------------------

            AppendAyat(
                output,
                ayat,
                options,
                surahNames);

            previousSurah = ayat.Surah;
        }

        return output
            .ToString()
            .Trim();
    }

    /// <summary>
    /// Format ayat menjadi CopyResult yang mempertahankan
    /// informasi font untuk setiap block.
    /// </summary>
    public CopyResult FormatBlocks(
        IReadOnlyList<Ayat> ayats,
        CopyOptions options,
        IReadOnlyDictionary<int, string>? surahNames = null)
    {
        if (ayats is null || ayats.Count == 0)
            return new CopyResult();

        if (options is null)
            throw new ArgumentNullException(nameof(options));

        var result = new CopyResult();

        int? previousSurah = null;

        foreach (var ayat in ayats)
        {
            // -----------------------------------------------------
            // Jika berganti surah
            // -----------------------------------------------------

            bool isNewSurah =
                previousSurah.HasValue &&
                previousSurah.Value != ayat.Surah;

            if (isNewSurah)
            {
                AddNewSurahBlocks(
                    result,
                    ayat,
                    options,
                    surahNames);
            }

            // -----------------------------------------------------
            // Format satu ayat
            // -----------------------------------------------------

            AddAyatBlocks(
                result,
                ayat,
                options,
                surahNames);

            previousSurah = ayat.Surah;
        }

        return result;
    }

    private static void AddAyatBlocks(
    CopyResult result,
    Ayat ayat,
    CopyOptions options,
    IReadOnlyDictionary<int, string>? surahNames)
    {
        bool hasArabic =
            options.IncludeArabic &&
            !string.IsNullOrWhiteSpace(ayat.Teks);

        bool hasTranslation =
            options.IncludeTranslation &&
            !string.IsNullOrWhiteSpace(ayat.Terjemahan);

        bool hasSignLanguage =
            options.IncludeSignLanguage &&
            !string.IsNullOrWhiteSpace(ayat.Isyarat);

        // ---------------------------------------------------------
        // SELANG-SELING
        // ---------------------------------------------------------

        if (options.AlternateContent)
        {
            AddAlternateBlocks(
                result,
                ayat,
                options,
                hasArabic,
                hasTranslation,
                hasSignLanguage,
                surahNames);

            return;
        }

        // ---------------------------------------------------------
        // ARAB
        // ---------------------------------------------------------

        if (hasArabic)
        {
            result.Blocks.Add(
                new CopyBlock
                {
                    Text = BuildArabicText(
                        ayat,
                        options),

                    Type = CopyBlockType.Arabic,

                    FontFamily = GetArabicFontFamily(
                        ayat,
                        options),

                    FontSize = 18,

                    IsRightToLeft = true
                });
        }

        // ---------------------------------------------------------
        // TERJEMAHAN
        // ---------------------------------------------------------

        if (hasTranslation)
        {
            result.Blocks.Add(
                new CopyBlock
                {
                    Text = ayat.Terjemahan.Trim(),

                    Type = CopyBlockType.Translation,

                    FontFamily = null,

                    FontSize = 11,

                    IsRightToLeft = false
                });

            // Footnote otomatis
            if (!string.IsNullOrWhiteSpace(ayat.TextFn))
            {
                result.Blocks.Add(
                    new CopyBlock
                    {
                        Text = ayat.TextFn.Trim(),

                        Type = CopyBlockType.Footnote,

                        FontFamily = null,

                        FontSize = 10,

                        IsRightToLeft = false
                    });
            }
        }

        // ---------------------------------------------------------
        // BAHASA ISYARAT
        // ---------------------------------------------------------

        if (hasSignLanguage)
        {
            result.Blocks.Add(
                new CopyBlock
                {
                    Text = ayat.Isyarat.Trim(),

                    Type = CopyBlockType.SignLanguage,

                    FontFamily = "LPMQ-ISYARAT",

                    FontSize = 18,

                    IsRightToLeft = false
                });
        }

        // ---------------------------------------------------------
        // SURAH : AYAT
        // ---------------------------------------------------------

        if (options.AddSurahAyah)
        {
            string surahName =
                GetSurahName(
                    ayat.Surah,
                    surahNames);

            result.Blocks.Add(
                new CopyBlock
                {
                    Text =
                        $"({surahName}/{ayat.Surah}: {ayat.AyatNumber})",

                    Type = CopyBlockType.SurahInfo,

                    FontFamily = null,

                    FontSize = 10,

                    IsRightToLeft = false
                });
        }

        // Separator antar ayat
        result.Blocks.Add(
            new CopyBlock
            {
                Text = string.Empty,
                Type = CopyBlockType.Separator
            });
    }

    private static string GetArabicFontFamily(
    Ayat ayat,
    CopyOptions options)
    {
        // ---------------------------------------------------------
        // COMPUTER FONT
        // ---------------------------------------------------------

        if (options.ArabicFont == ArabicCopyFont.ComputerFont)
        {
            return "LPMQ Isep Misbah";
        }

        // ---------------------------------------------------------
        // HANDWRITTEN / SYMBOL FONT
        // ---------------------------------------------------------

        if (ayat.KodeFont > 0)
        {
            return $"Lpmq_Hafs_{ayat.KodeFont:00}";
        }

        // ---------------------------------------------------------
        // Fallback
        // ---------------------------------------------------------

        return "LPMQ Isep Misbah";
    }

    // =============================================================
    // AYAT
    // =============================================================

    private static void AppendAyat(
        StringBuilder output,
        Ayat ayat,
        CopyOptions options,
        IReadOnlyDictionary<int, string>? surahNames)
    {
        bool hasArabic =
            options.IncludeArabic &&
            !string.IsNullOrWhiteSpace(ayat.Teks);

        bool hasTranslation =
            options.IncludeTranslation &&
            !string.IsNullOrWhiteSpace(ayat.Terjemahan);

        bool hasSignLanguage =
            options.IncludeSignLanguage &&
            !string.IsNullOrWhiteSpace(ayat.Isyarat);

        // ---------------------------------------------------------
        // SELANG-SELING
        // ---------------------------------------------------------

        if (options.AlternateContent)
        {
            AppendAlternateContent(
                output,
                ayat,
                options,
                hasArabic,
                hasTranslation,
                hasSignLanguage,
                surahNames);

            return;
        }

        // ---------------------------------------------------------
        // MODE BIASA
        // ---------------------------------------------------------

        if (hasArabic)
        {
            var arabic = BuildArabicText(
                ayat,
                options);

            output.AppendLine(arabic);
        }

        if (hasTranslation)
        {
            AppendTranslation(
                output,
                ayat);
        }

        if (hasSignLanguage)
        {
            AppendSignLanguage(
                output,
                ayat);
        }

        AppendSurahAyahIfNeeded(
            output,
            ayat,
            options,
            surahNames);

        output.AppendLine();
    }

    // =============================================================
    // ARAB
    // =============================================================

    private static string BuildArabicText(
        Ayat ayat,
        CopyOptions options)
    {
        string text = ayat.Teks.Trim();

        // ---------------------------------------------------------
        // Tanda kurung
        // ---------------------------------------------------------

        if (options.AddParentheses)
        {
            text =
                $"{OpenParenthesis} {text} {CloseParenthesis}";
        }

        // ---------------------------------------------------------
        // Nomor ayat
        // ---------------------------------------------------------

        if (options.AddAyahNumber)
        {
            string number =
                ArabicNumberHelper.ToArabicNumber(
                    ayat.AyatNumber);

            text += $" {number}";
        }

        return text;
    }

    // =============================================================
    // TERJEMAHAN
    // =============================================================

    private static void AppendTranslation(
        StringBuilder output,
        Ayat ayat)
    {
        if (string.IsNullOrWhiteSpace(ayat.Terjemahan))
            return;

        output.AppendLine(
            ayat.Terjemahan.Trim());

        // ---------------------------------------------------------
        // Catatan kaki otomatis
        //
        // Catatan kaki TIDAK mempunyai checkbox sendiri.
        // Jika terjemahan memiliki footnote, ikut disertakan.
        // ---------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(ayat.TextFn))
        {
            output.AppendLine();
            output.AppendLine(
                ayat.TextFn.Trim());
        }

        output.AppendLine();
    }

    // =============================================================
    // ISYARAT
    // =============================================================

    private static void AppendSignLanguage(
        StringBuilder output,
        Ayat ayat)
    {
        output.AppendLine(
            ayat.Isyarat.Trim());

        output.AppendLine();
    }

    // =============================================================
    // SELANG-SELING
    // =============================================================

    private static void AppendAlternateContent(
        StringBuilder output,
        Ayat ayat,
        CopyOptions options,
        bool hasArabic,
        bool hasTranslation,
        bool hasSignLanguage,
        IReadOnlyDictionary<int, string>? surahNames)
    {
        if (hasArabic)
        {
            output.AppendLine(
                BuildArabicText(
                    ayat,
                    options));

            output.AppendLine();
        }

        // ---------------------------------------------------------
        // Terjemahan
        // ---------------------------------------------------------

        if (hasTranslation)
        {
            AppendTranslation(
                output,
                ayat);
        }

        // ---------------------------------------------------------
        // Bahasa Isyarat
        // ---------------------------------------------------------

        if (hasSignLanguage)
        {
            AppendSignLanguage(
                output,
                ayat);
        }

        // ---------------------------------------------------------
        // Surah : Ayat
        // ---------------------------------------------------------

        AppendSurahAyahIfNeeded(
            output,
            ayat,
            options,
            surahNames);

        output.AppendLine();
    }

    private static void AddNewSurahBlocks(
    CopyResult result,
    Ayat ayat,
    CopyOptions options,
    IReadOnlyDictionary<int, string>? surahNames)
    {
        // ---------------------------------------------------------
        // Separator pergantian surah
        // ---------------------------------------------------------

        result.Blocks.Add(
            new CopyBlock
            {
                Text = string.Empty,
                Type = CopyBlockType.Separator
            });

        result.Blocks.Add(
            new CopyBlock
            {
                Text = string.Empty,
                Type = CopyBlockType.Separator
            });

        // ---------------------------------------------------------
        // Informasi surah
        // ---------------------------------------------------------

        if (options.AddSurahAyah)
        {
            string surahName =
                GetSurahName(
                    ayat.Surah,
                    surahNames);

            result.Blocks.Add(
                new CopyBlock
                {
                    Text = $"[{surahName}]",
                    Type = CopyBlockType.SurahInfo,
                    FontSize = 11,
                    IsRightToLeft = false
                });
        }

        // ---------------------------------------------------------
        // Bismillah
        // ---------------------------------------------------------

        if (ayat.Surah != 1 &&
            ayat.AyatNumber == 1 &&
            options.IncludeArabic)
        {
            string bismillah =
                "بِسْمِ اللّٰهِ الرَّحْمٰنِ الرَّحِيْمِ";

            if (options.AddParentheses)
            {
                bismillah =
                    $"{OpenParenthesis} " +
                    $"{bismillah} " +
                    $"{CloseParenthesis}";
            }

            result.Blocks.Add(
                new CopyBlock
                {
                    Text = bismillah,

                    Type = CopyBlockType.Arabic,

                    FontFamily = GetArabicFontFamily(
                        ayat,
                        options),

                    FontSize = 18,

                    IsRightToLeft = true
                });
        }
    }

    private static void AddAlternateBlocks(
    CopyResult result,
    Ayat ayat,
    CopyOptions options,
    bool hasArabic,
    bool hasTranslation,
    bool hasSignLanguage,
    IReadOnlyDictionary<int, string>? surahNames)
    {
        // ---------------------------------------------------------
        // Arab
        // ---------------------------------------------------------

        if (hasArabic)
        {
            result.Blocks.Add(
                new CopyBlock
                {
                    Text = BuildArabicText(
                        ayat,
                        options),

                    Type = CopyBlockType.Arabic,

                    FontFamily = GetArabicFontFamily(
                        ayat,
                        options),

                    FontSize = 18,

                    IsRightToLeft = true
                });
        }

        // ---------------------------------------------------------
        // Terjemahan
        // ---------------------------------------------------------

        if (hasTranslation)
        {
            result.Blocks.Add(
                new CopyBlock
                {
                    Text = ayat.Terjemahan.Trim(),

                    Type = CopyBlockType.Translation,

                    FontSize = 11,

                    IsRightToLeft = false
                });

            if (!string.IsNullOrWhiteSpace(ayat.TextFn))
            {
                result.Blocks.Add(
                    new CopyBlock
                    {
                        Text = ayat.TextFn.Trim(),

                        Type = CopyBlockType.Footnote,

                        FontSize = 10,

                        IsRightToLeft = false
                    });
            }
        }

        // ---------------------------------------------------------
        // Bahasa Isyarat
        // ---------------------------------------------------------

        if (hasSignLanguage)
        {
            result.Blocks.Add(
                new CopyBlock
                {
                    Text = ayat.Isyarat.Trim(),

                    Type = CopyBlockType.SignLanguage,

                    FontFamily = "LPMQ-ISYARAT",

                    FontSize = 18,

                    IsRightToLeft = false
                });
        }

        // ---------------------------------------------------------
        // Surah : Ayat
        // ---------------------------------------------------------

        if (options.AddSurahAyah)
        {
            string surahName =
                GetSurahName(
                    ayat.Surah,
                    surahNames);

            result.Blocks.Add(
                new CopyBlock
                {
                    Text =
                        $"({surahName}/{ayat.Surah}: {ayat.AyatNumber})",

                    Type = CopyBlockType.SurahInfo,

                    FontSize = 10,

                    IsRightToLeft = false
                });
        }

        result.Blocks.Add(
            new CopyBlock
            {
                Text = string.Empty,
                Type = CopyBlockType.Separator
            });
    }

    // =============================================================
    // SURAH : AYAT
    // =============================================================

    private static void AppendSurahAyahIfNeeded(
        StringBuilder output,
        Ayat ayat,
        CopyOptions options,
        IReadOnlyDictionary<int, string>? surahNames)
    {
        if (!options.AddSurahAyah)
            return;

        string surahName =
            GetSurahName(
                ayat.Surah,
                surahNames);

        string ayahNumber =
            ayat.AyatNumber.ToString();

        output.AppendLine(
            $"({surahName}/{ayat.Surah}: {ayahNumber})");
    }

    private static string GetSurahName(
        int surahNumber,
        IReadOnlyDictionary<int, string>? surahNames)
    {
        if (surahNames is not null &&
            surahNames.TryGetValue(
                surahNumber,
                out var name) &&
            !string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        return $"Surah {surahNumber}";
    }

    // =============================================================
    // PERGANTIAN SURAH
    // =============================================================

    private static void AppendNewSurahSeparator(
        StringBuilder output,
        Ayat ayat,
        CopyOptions options,
        IReadOnlyDictionary<int, string>? surahNames)
    {
        output.AppendLine();
        output.AppendLine();

        // ---------------------------------------------------------
        // Informasi surah jika diminta
        // ---------------------------------------------------------

        if (options.AddSurahAyah)
        {
            string surahName =
                GetSurahName(
                    ayat.Surah,
                    surahNames);

            output.AppendLine(
                $"[{surahName}]");

            output.AppendLine();
        }

        // ---------------------------------------------------------
        // Bismillah
        //
        // Al-Fatihah tidak perlu ditambahkan karena ayat 1
        // memang merupakan Bismillah.
        //
        // Untuk surah lain, Bismillah akan ditambahkan saat
        // range memasuki ayat pertama surah tersebut.
        // ---------------------------------------------------------

        if (ayat.Surah != 1 &&
            ayat.AyatNumber == 1 &&
            options.IncludeArabic)
        {
            string bismillah =
                "بِسْمِ اللّٰهِ الرَّحْمٰنِ الرَّحِيْمِ";

            if (options.AddParentheses)
            {
                bismillah =
                    $"{OpenParenthesis} " +
                    $"{bismillah} " +
                    $"{CloseParenthesis}";
            }

            output.AppendLine(
                bismillah);

            output.AppendLine();
        }
    }
}