using QuranKemenag.Converters;
using QuranKemenag.Models;
using QuranKemenag.Repositories;
using System.Globalization;

namespace QuranKemenag.Services.Page;

public class MushafPageService
{
    private readonly LayoutRepository _layoutRepository;
    private readonly IQuranRepository _quranRepository;
    private readonly ISymbolRepository _symbolRepository;

    private readonly AyatNumberConverter _ayatNumberConverter = new();

    /// <summary>
    /// Diagnostik sementara: mencatat isi mentah Teks yang berhasil/gagal
    /// diambil per ayat pada pemanggilan GetPageAsync terakhir.
    /// </summary>
    public List<string> LastDiagnostics { get; } = new();

    public MushafPageService(
        LayoutRepository layoutRepository,
        IQuranRepository quranRepository,
        ISymbolRepository symbolRepository)
    {
        _layoutRepository = layoutRepository;
        _quranRepository = quranRepository;
        _symbolRepository = symbolRepository;
    }

    public List<PageLine> GetPage(int pageNumber)
    {
        return _layoutRepository.GetPageLines(pageNumber);
    }

    /// <summary>
    /// Mengambil satu halaman Mushaf sesuai mode tampilan:
    /// - ComputerFont          → teks dari quran.kemenag.
    /// - HandwrittenManuscript → teks dari msi_simbol.db.
    /// Pembagian baris/kata SELALU dari mushaf_layout.db, tidak berubah.
    /// </summary>
    public async Task<List<PageLine>> GetPageAsync(
        int pageNumber,
        DisplayMode mode)
    {
        LastDiagnostics.Clear();
        var lines = _layoutRepository.GetPageLines(pageNumber);

        var textCache = new Dictionary<(int Surah, int Ayat), List<string>>();
        var fontCache = new Dictionary<(int Surah, int Ayat), int?>();

        foreach (var line in lines)
        {
            foreach (var word in line.Words)
            {
                var key = (word.SurahNumber, word.AyatNumber);

                if (!textCache.TryGetValue(key, out var wordsInAyat))
                {
                    if (mode == DisplayMode.ComputerFont)
                    {
                        var ayat = await _quranRepository
                            .GetAyatAsync(word.SurahNumber, word.AyatNumber)
                            .ConfigureAwait(false);

                        wordsInAyat = SplitAyatIntoWords(ayat?.Teks);
                        fontCache[key] = 0;
                    }
                    else
                    {
                        var ayatSymbol = await _symbolRepository
                            .GetAyatAsync(word.SurahNumber, word.AyatNumber)
                            .ConfigureAwait(false);

                        // ===== DIAGNOSTIK =====
                        var teksValue = ayatSymbol?.Teks;

                        var hexDump = teksValue is null
                            ? "(null)"
                            : string.Join(
                                " ",
                                teksValue.Select(c => $"U+{(int)c:X4}"));

                        LastDiagnostics.Add(
                            $"Ayat {word.SurahNumber}:{word.AyatNumber} => " +
                            $"Found={(ayatSymbol is not null)}, KodeFont={ayatSymbol?.KodeFont}, " +
                            $"Length={teksValue?.Length ?? -1}");
                        LastDiagnostics.Add($"    HEX: {hexDump}");
                        // ===== AKHIR DIAGNOSTIK =====

                        wordsInAyat = SplitAyatIntoWords(ayatSymbol?.Teks);
                        // msi_simbol.db menaruh 1 glyph EKSTRA di akhir setiap ayat — simbol
                        // lingkaran nomor ayat versi tulisan tangan (bukan kata Quran).
                        // mushaf_layout.db tidak menghitung ini sebagai kata, jadi kita
                        // gabungkan ke kata terakhir yang sesungguhnya, supaya WordNumber
                        // tetap sinkron DAN nomor ayat tetap tampil (pakai glyph ASLI
                        // manuskrip, bukan simbol ۝ buatan kita).
                        if (wordsInAyat.Count > 1)
                        {
                            var lastIndex = wordsInAyat.Count - 1;
                            wordsInAyat[lastIndex - 1] =
                                wordsInAyat[lastIndex - 1] + " " + wordsInAyat[lastIndex];
                            wordsInAyat.RemoveAt(lastIndex);
                        }

                        fontCache[key] = ayatSymbol?.KodeFont;
                    }

                    textCache[key] = wordsInAyat;
                }

                var index = word.WordNumber - 1;
                word.KodeFont = fontCache[key];

                if (index >= 0 && index < wordsInAyat.Count)
                {
                    var text = wordsInAyat[index];

                    // Tanda akhir ayat (۝) hanya disintesis untuk mode
                    // Computer Font. Untuk Handwritten, tanda semacam ini
                    // (kalau ada) harus datang dari msi_simbol.db sendiri —
                    // kita tidak menambah-nambahi data manuskrip.
                    //if (mode == DisplayMode.ComputerFont &&
                    //    word.WordNumber == wordsInAyat.Count)
                    //{
                    //    var marker = _ayatNumberConverter.Convert(
                    //        word.AyatNumber, typeof(string), null!, "") as string;

                    //    text += marker;
                    //}

                    // SESUDAH
                    if (word.WordNumber == wordsInAyat.Count)

                        if (mode == DisplayMode.ComputerFont && word.WordNumber == wordsInAyat.Count)
                    {
                        word.UnicodeText = text;
                        word.SymbolText = null;
                    }
                    else
                    {
                        word.SymbolText = text;
                        word.UnicodeText = null;
                    }

                    word.DisplayText = text;
                }
                else
                {
                    // Data untuk mode ini belum ada di sumbernya (quran.kemenag /
                    // msi_simbol.db). Pakai WordText (mushaf_layout.db) sebagai
                    // cadangan, tapi tetap percantik tanda akhir ayat-nya supaya
                    // konsisten dengan mode Computer Font (bukan cuma angka polos).
                    word.UnicodeText = null;
                    word.SymbolText = null;
                    word.IsFallback = true;
                    word.DisplayText = FormatFallbackText(word);
                }
            }
        }

        return lines;
    }

    private static List<string> SplitAyatIntoWords(string? teks)
    {
        var result = new List<string>();

        if (string.IsNullOrWhiteSpace(teks))
        {
            return result;
        }

        var rawTokens = teks.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var token in rawTokens)
        {
            if (IsWaqfMarkOnly(token))
            {
                if (result.Count > 0)
                {
                    result[^1] = result[^1] + " " + token;
                }
                continue;
            }

            result.Add(token);
        }
        ReattachLeadingMarks(result);

        return result;
    }

    private static bool IsWaqfMarkOnly(string token)
    {
        foreach (var c in token)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);

            if (category != UnicodeCategory.NonSpacingMark && c != '\u06D5')
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Beberapa tanda kecil (misalnya ۙ) di quran.kemenag ditulis MENEMPEL
    /// di awal kata berikutnya tanpa spasi (contoh: "ۙاِنَّمَا"). Kalau
    /// dirender apa adanya, tanda ini jadi "yatim" (tidak ada huruf dasar
    /// di depannya dalam TextBlock yang sama) dan muncul sebagai kotak
    /// kosong. Method ini memindahkan tanda tersebut ke AKHIR kata
    /// SEBELUMNYA, supaya tetap tampil tapi tidak jadi yatim.
    /// </summary>
    private static void ReattachLeadingMarks(List<string> words)
    {
        for (var i = 1; i < words.Count; i++)
        {
            var token = words[i];
            var leadCount = 0;

            while (leadCount < token.Length &&
                   CharUnicodeInfo.GetUnicodeCategory(token[leadCount])
                       == UnicodeCategory.NonSpacingMark)
            {
                leadCount++;
            }

            if (leadCount > 0 && leadCount < token.Length)
            {
                var leadingMarks = token[..leadCount];
                words[i - 1] = words[i - 1] + " " + leadingMarks;
                words[i] = token[leadCount..];
            }
        }
    }

    /// <summary>
    /// mushaf_layout.db sudah menaruh angka ayat mentah di ujung kata
    /// terakhir suatu ayat (contoh: "يُؤْمِنُونَ ٦"). Method ini mendeteksi
    /// digit Arab (٠-٩) di ujung teks, membuangnya, lalu menggantinya
    /// dengan simbol akhir ayat yang sama seperti mode Computer Font (۝).
    /// </summary>
    private string FormatFallbackText(PageWord word)
    {
        var text = word.WordText;

        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var trimmed = text.TrimEnd();
        var lastChar = trimmed[^1];

        var endsWithArabicDigit =
            lastChar >= '\u0660' && lastChar <= '\u0669';

        if (!endsWithArabicDigit)
        {
            return text;
        }

        var cutIndex = trimmed.Length;

        while (cutIndex > 0 &&
               (trimmed[cutIndex - 1] is (>= '\u0660' and <= '\u0669') or ' '))
        {
            cutIndex--;
        }

        var baseText = trimmed[..cutIndex].TrimEnd();

        var marker = _ayatNumberConverter.Convert(
            word.AyatNumber, typeof(string), null!, "") as string;

        return baseText + marker;
    }
}