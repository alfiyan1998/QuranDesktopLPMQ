using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

using QuranKemenag.Models;

using System;
using System.Collections.Generic;

using Windows.Foundation;

namespace QuranKemenag.Services.Page;

// =====================================================================
// HANDWRITTEN PAGE LAYOUT ENGINE
// =====================================================================
//
// Tujuan file ini: mengubah ayat.Teks (Handwritten / KodeFont != 0),
// yang berisi rangkaian codepoint Private Use Area (PUA), menjadi
// data posisi (x, y) per-glyph yang siap ditempel ke sebuah Canvas.
//
// PUA di sini TIDAK diperlakukan sebagai teks Unicode biasa:
// - tidak ada Paragraph/Run/TextAlignment.Justify
// - tidak ada RTL mark (U+200F) atau Unicode BiDi
// - urutan & posisi visual dihitung manual oleh engine ini
//
// Alasan: PUA (U+E000-F8FF) memiliki BiDi class default "L" (Left-to-
// Right), sehingga runtime BiDi memperlakukan setiap rangkaian PUA
// sebagai "pulau" LTR di dalam paragraf RTL, dan itulah sumber utama
// glyph yang berpindah posisi / urutan yang tidak konsisten pada
// implementasi sebelumnya. Engine ini menggantikan BiDi sepenuhnya
// untuk konten Handwritten.
//
// PENTING - URUTAN GLYPH DI DALAM ayat.Teks:
// Data mentah di msi_simbol.db (kolom teks) TERNYATA SUDAH dibalik
// oleh pipeline yang ada sebelum sampai ke sini:
//
//     SymbolRepository (raw DB order)
//        -> SymbolTextProcessor.Process()
//           -> SymbolTextHelper.ReverseCharacters(teks)   <-- di sini
//        -> Ayat.Teks
//
// (lihat Services/Text/SymbolTextProcessor.cs dan
// Helpers/SymbolTextHelper.cs - komentar aslinya menyebut reversal ini
// sebagai perbaikan urutan kata yang sudah ada di database Symbol).
// Karena itu, ayat.Teks yang diterima MushafPageView SUDAH berada
// dalam urutan baca (reading order) yang benar: karakter pertama pada
// string = kata yang pertama dibaca = harus diletakkan PALING KANAN.
// Engine ini TIDAK membalik lagi urutan tersebut secara default.
//
// Jika hasil visual test Al-Fatihah (LANGKAH 9) menunjukkan urutan
// masih terbalik, ubah HandwrittenLayoutEngine.ReverseGlyphOrderPerAyat
// menjadi true - satu baris, tidak perlu mengubah algoritma lain.
// =====================================================================

/// <summary>
/// Satu unit visual atomik: satu codepoint PUA (satu kata/glyph
/// artwork Handwritten), lengkap dengan font dan ukuran terukurnya.
/// </summary>
public sealed class HandwrittenGlyph
{
    public char Character { get; init; }

    public FontFamily Font { get; init; } = null!;

    public double Width { get; set; }

    public double Height { get; set; }
}


/// <summary>
/// Satu glyph yang sudah diberi koordinat final (relatif terhadap
/// pojok kiri-atas area konten / Canvas).
/// </summary>
public sealed class PositionedGlyph
{
    public HandwrittenGlyph Glyph { get; init; } = null!;

    public double X { get; set; }

    public double Y { get; set; }
}


/// <summary>
/// Satu baris hasil line-breaking manual, berisi glyph-glyph yang
/// sudah diposisikan secara RTL (kanan -> kiri).
/// </summary>
public sealed class HandwrittenLine
{
    public List<PositionedGlyph> Glyphs { get; } = new();

    public double Height { get; set; }
}


/// <summary>
/// Hasil akhir layout satu halaman Handwritten: daftar baris + tinggi
/// total (dipakai untuk mengatur Canvas.Height).
/// </summary>
public sealed class HandwrittenPageLayout
{
    public List<HandwrittenLine> Lines { get; } = new();

    public double TotalHeight { get; set; }
}


/// <summary>
/// Kontrak pengukur ukuran glyph aktual (lebar/tinggi) untuk font dan
/// ukuran tertentu. Dipisah sebagai interface supaya
/// <see cref="HandwrittenLayoutEngine"/> tidak terikat langsung pada
/// cara pengukuran (saat ini: TextBlock.Measure sementara, lihat
/// <see cref="TextBlockGlyphMeasurer"/>).
/// </summary>
public interface IGlyphMeasurer
{
    Size Measure(char character, FontFamily font, double fontSize);
}


/// <summary>
/// Implementasi pengukur menggunakan satu TextBlock yang dipakai
/// ulang (tidak ditambahkan ke visual tree) untuk mendapatkan ukuran
/// render aktual dari sebuah glyph PUA pada font Handwritten tertentu.
///
/// Ini persis rekomendasi LANGKAH 3: "Render glyph sementara untuk
/// mendapatkan ukuran aktual" - tanpa mengubah font yang dipakai.
/// </summary>
public sealed class TextBlockGlyphMeasurer : IGlyphMeasurer
{
    private readonly TextBlock _probe = new TextBlock();

    public Size Measure(char character, FontFamily font, double fontSize)
    {
        _probe.FontFamily = font;
        _probe.FontSize = fontSize;
        _probe.Text = character.ToString();

        _probe.Measure(
            new Size(
                double.PositiveInfinity,
                double.PositiveInfinity));

        var size = _probe.DesiredSize;

        // Fallback konservatif kalau pengukuran gagal memberi ukuran
        // (misalnya font belum termuat saat pass pertama) supaya
        // glyph tidak saling tumpuk di titik yang sama.
        if (size.Width <= 0 || size.Height <= 0)
        {
            return new Size(
                fontSize * 0.6,
                fontSize * 1.1);
        }

        return size;
    }
}


public static class HandwrittenLayoutEngine
{
    // Lihat catatan besar di atas file ini.
    //
    // UPDATE setelah visual test Al-Fatihah (screenshot dari user):
    // urutan glyph TERNYATA masih terbalik dengan asumsi awal
    // (ReverseGlyphOrderPerAyat = false, mengandalkan reversal yang
    // sudah dilakukan SymbolTextHelper.ReverseCharacters di upstream).
    // Kemungkinan reversal upstream tersebut sebenarnya dimaksudkan
    // untuk pipeline lain (mis. mode Ayat/Surah berbasis List, bukan
    // Canvas manual), atau karakter-per-karakter reversal itu sendiri
    // yang justru perlu "dibatalkan" lagi oleh reversal kedua di sini
    // supaya kembali ke urutan baca yang benar untuk renderer Canvas
    // ini. Diset true berdasarkan bukti visual langsung (lebih kuat
    // daripada asumsi dari membaca kode upstream).
    public const bool ReverseGlyphOrderPerAyat = true;

    // Jarak antar glyph. Mulai kecil sesuai LANGKAH 6 (0-8px), bisa
    // dituning setelah terlihat hasil render sebenarnya.
    public const double DefaultGlyphSpacing = 6.0;

    // Pengali tinggi baris terhadap FontSize. Sama dengan nilai yang
    // sebelumnya sudah dituning untuk Paragraph.LineHeight pada
    // RichTextBlock (lihat komentar lama di MushafPageView.xaml.cs).
    public const double DefaultLineHeightMultiplier = 1.2;


    /// <summary>
    /// Membangun layout penuh (baris + posisi tiap glyph) untuk semua
    /// ayat Handwritten pada satu halaman, sebagai satu aliran glyph
    /// yang berkesinambungan (batas ayat tidak memaksa baris baru -
    /// LANGKAH 7).
    /// </summary>
    /// <param name="ayats">
    /// Ayat Handwritten (KodeFont != 0) pada halaman, dalam urutan
    /// tampil (Surah, lalu AyatNumber).
    /// </param>
    /// <param name="fontResolver">
    /// Mengembalikan FontFamily untuk sebuah kode_font. Wajib sama
    /// dengan yang dipakai renderer lama (manager.GetSymbolFont).
    /// </param>
    public static HandwrittenPageLayout BuildLayout(
        IReadOnlyList<Ayat> ayats,
        Func<int, FontFamily> fontResolver,
        double fontSize,
        double availableWidth,
        IGlyphMeasurer measurer,
        double glyphSpacing = DefaultGlyphSpacing,
        double lineHeightMultiplier = DefaultLineHeightMultiplier)
    {
        var layout = new HandwrittenPageLayout();

        if (ayats is null ||
            ayats.Count == 0 ||
            availableWidth <= 0)
        {
            return layout;
        }

        // -----------------------------------------------------------
        // LANGKAH 1 - EXTRACT GLYPH
        // -----------------------------------------------------------
        // Satu daftar glyph datar untuk SELURUH halaman. Batas ayat
        // TIDAK disisipkan sebagai objek apa pun (tidak ada line
        // break paksa, tidak ada spacing ekstra) - hanya lanjutan
        // dari glyph terakhir ayat sebelumnya, persis seperti
        // hubungan antar-kata di dalam satu ayat.
        // -----------------------------------------------------------
        var glyphs = new List<HandwrittenGlyph>();

        foreach (var ayat in ayats)
        {
            if (ayat is null || ayat.KodeFont == 0)
                continue;

            // Trim hanya menormalkan string transient di memori
            // (beberapa baris menyimpan spasi duplikat di kedua sisi).
            // Database / ayat.Teks tidak diubah.
            var text = ayat.Teks?.Trim() ?? string.Empty;

            if (text.Length == 0)
                continue;

            var font = fontResolver(ayat.KodeFont);

            var ayatGlyphs = new List<HandwrittenGlyph>(text.Length);

            foreach (var c in text)
            {
                // Whitespace (termasuk NBSP U+00A0 yang dipakai
                // sebagai separator pada sebagian baris) hanya
                // pemisah, bukan glyph yang dirender.
                if (char.IsWhiteSpace(c))
                    continue;

                var size = measurer.Measure(c, font, fontSize);

                ayatGlyphs.Add(
                    new HandwrittenGlyph
                    {
                        Character = c,
                        Font = font,
                        Width = size.Width,
                        Height = size.Height
                    });
            }

            if (ReverseGlyphOrderPerAyat)
            {
                ayatGlyphs.Reverse();
            }

            glyphs.AddRange(ayatGlyphs);
        }

        if (glyphs.Count == 0)
            return layout;

        // -----------------------------------------------------------
        // LANGKAH 4 & 5 - LINE BREAK + ARAH BARIS (manual, tanpa BiDi)
        // -----------------------------------------------------------
        double defaultLineHeight = fontSize * lineHeightMultiplier;

        var currentLine = new HandwrittenLine { Height = defaultLineHeight };
        double x = availableWidth;
        double usedWidth = 0;

        foreach (var glyph in glyphs)
        {
            bool wouldOverflow =
                usedWidth > 0 &&
                usedWidth + glyphSpacing + glyph.Width > availableWidth;

            if (wouldOverflow)
            {
                layout.Lines.Add(currentLine);

                currentLine = new HandwrittenLine { Height = defaultLineHeight };
                x = availableWidth;
                usedWidth = 0;
            }

            if (usedWidth > 0)
            {
                x -= glyphSpacing;
                usedWidth += glyphSpacing;
            }

            x -= glyph.Width;

            currentLine.Glyphs.Add(
                new PositionedGlyph
                {
                    Glyph = glyph,
                    X = x,
                    Y = 0 // diisi di bawah, setelah tinggi tiap baris diketahui
                });

            currentLine.Height = Math.Max(currentLine.Height, glyph.Height);

            usedWidth += glyph.Width;
        }

        if (currentLine.Glyphs.Count > 0)
        {
            layout.Lines.Add(currentLine);
        }

        double y = 0;

        foreach (var line in layout.Lines)
        {
            foreach (var positioned in line.Glyphs)
            {
                positioned.Y = y;
            }

            y += line.Height;
        }

        layout.TotalHeight = y;

        return layout;
    }
}