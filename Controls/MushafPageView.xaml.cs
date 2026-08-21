using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

using QuranKemenag.Models;
using QuranKemenag.Services.Font;
using QuranKemenag.Services.Page;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace QuranKemenag.Views.Controls;

public sealed partial class MushafPageView : UserControl
{
    // Dipakai ulang untuk semua pengukuran glyph Handwritten pada
    // instance control ini (lihat HandwrittenLayoutEngine.cs).
    private readonly IGlyphMeasurer _glyphMeasurer =
        new TextBlockGlyphMeasurer();

    // Halaman yang sedang aktif, disimpan supaya bisa di-render ulang
    // saat PART_ContentFrame baru mendapatkan ActualWidth yang valid
    // (Canvas, tidak seperti RichTextBlock, butuh lebar numerik untuk
    // bisa melakukan line-breaking manual).
    private QuranPage? _currentPage;

    private bool _contentFrameSizeHandlerAttached;


    public MushafPageView()
    {
        InitializeComponent();

        DataContextChanged +=
            MushafPageView_DataContextChanged;
    }


    private void MushafPageView_DataContextChanged(
        FrameworkElement sender,
        DataContextChangedEventArgs args)
    {
        if (args.NewValue is not QuranPage page)
        {
            _currentPage = null;

            PART_RichText.Blocks.Clear();
            PART_RichText.Visibility = Visibility.Collapsed;

            PART_HandwrittenCanvas.Children.Clear();
            PART_HandwrittenCanvas.Visibility = Visibility.Collapsed;

            return;
        }

        Debug.WriteLine(
            $"[PAGE VIEW] " +
            $"Page={page.PageNumber} " +
            $"Surah={page.SurahNumber} " +
            $"Juz={page.Juz} " +
            $"AyatCount={page.Ayats?.Count ?? 0}");

        _currentPage = page;

        RenderPage(page);
    }


    // ------------------------------------------------------------
    // DISPATCH
    // ------------------------------------------------------------
    // Halaman dipecah menjadi ayat Computer Font (KodeFont == 0) dan
    // ayat Handwritten (KodeFont != 0). Masing-masing dirender oleh
    // jalur yang sepenuhnya terpisah:
    //   - RenderComputerFont: SAMA PERSIS dengan implementasi lama.
    //   - RenderHandwritten : engine baru (Canvas + posisi manual).
    //
    // Catatan jujur soal keterbatasan: jika suatu saat satu halaman
    // benar-benar berisi CAMPURAN Computer Font dan Handwritten, kedua
    // blok akan tampil bertumpuk secara vertikal (Computer Font di
    // atas, Handwritten di bawah) sesuai urutan StackPanel di XAML,
    // BUKAN diselang-seling sesuai urutan ayat aslinya. RichTextBlock
    // yang mengalir dan Canvas yang berposisi absolut tidak bisa
    // disatukan menjadi satu aliran baca tanpa menyatukan keduanya ke
    // satu engine (di luar scope permintaan ini, yang membatasi
    // perubahan hanya pada renderer Handwritten). Berdasarkan data
    // nyata yang diperiksa (msi_simbol.db), setiap halaman yang
    // memiliki data Handwritten sejauh ini seluruhnya Handwritten,
    // jadi kasus campuran ini belum pernah terjadi di data yang ada.
    // ------------------------------------------------------------
    private void RenderPage(
        QuranPage page)
    {
        var manager =
            App.Services?.GetService(
                typeof(IFontManager))
            as IFontManager;

        var ayats =
            page.Ayats
            ?? Array.Empty<Ayat>();

        var computerAyats =
            ayats
                .Where(a => a is not null && a.KodeFont == 0)
                .ToList();

        var handwrittenAyats =
            ayats
                .Where(a => a is not null && a.KodeFont != 0)
                .ToList();

        PART_RichText.Blocks.Clear();
        PART_HandwrittenCanvas.Children.Clear();

        if (computerAyats.Count > 0)
        {
            RenderComputerFont(computerAyats, manager);
            PART_RichText.Visibility = Visibility.Visible;
        }
        else
        {
            PART_RichText.Visibility = Visibility.Collapsed;
        }

        if (handwrittenAyats.Count > 0)
        {
            RenderHandwritten(handwrittenAyats, manager);
            PART_HandwrittenCanvas.Visibility = Visibility.Visible;
        }
        else
        {
            PART_HandwrittenCanvas.Visibility = Visibility.Collapsed;
        }
    }


    // ==============================================================
    // MODE COMPUTER FONT (KodeFont == 0) - TIDAK DIUBAH
    // ==============================================================
    // Logika di bawah ini adalah logika lama, dipindah apa adanya dari
    // RenderPage tanpa perubahan perilaku, hanya difilter agar hanya
    // memproses ayat Computer Font (dulu ini bercampur dengan buffer
    // Handwritten di method yang sama).
    // ==============================================================
    private void RenderComputerFont(
        IReadOnlyList<Ayat> ayats,
        IFontManager? manager)
    {
        var paragraph =
            new Paragraph
            {
                TextAlignment = TextAlignment.Justify,
                LineHeight = PART_RichText.FontSize * 1.2
            };

        const string RtlMark = "\u200F";

        foreach (var ayat in ayats)
        {
            if (ayat is null)
                continue;

            var arabicText =
                ayat.Teks?.Trim()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(arabicText))
                continue;

            var run =
                new Run
                {
                    Text =
                        RtlMark +
                        arabicText +
                        RtlMark,

                    FontSize =
                        PART_RichText.FontSize
                };

            ApplyAyatFont(
                run,
                ayat,
                manager);

            paragraph.Inlines.Add(run);

            if (!string.IsNullOrWhiteSpace(ayat.ArabicAyatNumber))
            {
                paragraph.Inlines.Add(
                    new Run { Text = " " });

                paragraph.Inlines.Add(
                    BuildAyatNumberBadge(ayat, manager));

                paragraph.Inlines.Add(
                    new Run { Text = " " });
            }
        }

        PART_RichText.Blocks.Add(paragraph);
    }


    // --------------------------------------------------------------
    // BADGE NOMOR AYAT (Computer Font)
    // --------------------------------------------------------------
    // Sebelumnya nomor ayat dirender sebagai teks polos
    // "۝" (U+06DD, End-of-Ayah) + angka Arab, dengan asumsi font
    // Unicode (LPMQ IsepMisbah) punya glyph rosette/lingkaran untuk
    // kombinasi itu. Dari screenshot: sebagian nomor tampil sebagai
    // lingkaran kecil yang benar, tapi ayat 5 justru tampil sebagai
    // angka besar polos tanpa lingkaran sama sekali - artinya font
    // tersebut tidak konsisten menyediakan glyph rosette untuk semua
    // digit, membuat halaman tampak berantakan.
    //
    // Solusi: gambar lingkaran itu sendiri (Ellipse + TextBlock di
    // dalam InlineUIContainer) alih-alih bergantung pada glyph font.
    // Hasilnya konsisten untuk semua nomor ayat, tidak peduli apakah
    // font Unicode punya glyph rosette atau tidak, dan warnanya
    // memakai LpmqGreenBrush yang sama dengan border halaman supaya
    // tetap serasi dengan tema aplikasi.
    // --------------------------------------------------------------
    private InlineUIContainer BuildAyatNumberBadge(
        Ayat ayat,
        IFontManager? manager)
    {
        double bodyFontSize = PART_RichText.FontSize;
        double badgeSize = Math.Max(20, bodyFontSize * 0.46);
        double numberFontSize = badgeSize * 0.52;

        var strokeBrush =
            Application.Current.Resources.TryGetValue(
                "LpmqGreenBrush",
                out var brushResource) &&
            brushResource is Brush resolvedBrush
                ? resolvedBrush
                : new SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 15, 107, 60));

        var ellipse =
            new Ellipse
            {
                Width = badgeSize,
                Height = badgeSize,
                Stroke = strokeBrush,
                StrokeThickness = 1.3,
                Fill = new SolidColorBrush(Microsoft.UI.Colors.Transparent)
            };

        var numberText =
            new TextBlock
            {
                Text = ayat.ArabicAyatNumber,
                FontSize = numberFontSize,
                Foreground = strokeBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };

        // Digit Arab (Arabic-Indic) tidak butuh font Handwritten/
        // Symbol khusus - font Unicode utama sudah cukup, tapi kita
        // tetap pakai ApplyAyatFont supaya konsisten dengan sisa teks
        // (mis. jika di masa depan proyek mengganti font Unicode).
        try
        {
            if (manager is not null)
            {
                numberText.FontFamily = manager.GetUnicodeFont();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"[PAGE FONT] " +
                $"Fallback font (ayat badge): {ex.Message}");
        }

        var badge =
            new Grid
            {
                Width = badgeSize,
                Height = badgeSize
            };

        badge.Children.Add(ellipse);
        badge.Children.Add(numberText);

        return new InlineUIContainer { Child = badge };
    }


    // ==============================================================
    // MODE HANDWRITTEN (KodeFont != 0) - RENDERER BARU
    // ==============================================================
    private void RenderHandwritten(
        IReadOnlyList<Ayat> ayats,
        IFontManager? manager)
    {
        double fontSize = PART_RichText.FontSize;

        double availableWidth =
                PART_ContentHost.ActualWidth;

        Debug.WriteLine(
    $"[HANDWRITTEN SIZE] " +
    $"Frame={PART_ContentFrame.ActualWidth:F1} " +
    $"Host={PART_ContentHost.ActualWidth:F1} " +
    $"Canvas={PART_HandwrittenCanvas.ActualWidth:F1}");

        if (availableWidth <= 0)
        {
            // Layout pass pertama belum selesai (ActualWidth masih 0).
            // Canvas, tidak seperti RichTextBlock, butuh angka pasti
            // untuk melakukan line-breaking manual, jadi kita pasang
            // handler satu-kali untuk merender ulang begitu ukuran
            // sebenarnya tersedia, dan untuk saat ini keluar tanpa
            // menggambar apa pun (menghindari layout yang salah karena
            // availableWidth = 0).
            AttachContentFrameSizeHandlerOnce();
            return;
        }

        FontFamily ResolveFont(int kodeFont)
        {
            try
            {
                return manager?.GetSymbolFont(kodeFont)
                    ?? new FontFamily("Segoe UI");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[PAGE FONT] " +
                    $"Fallback font (Handwritten): {ex.Message}");

                return new FontFamily("Segoe UI");
            }
        }

        var layout =
            HandwrittenLayoutEngine.BuildLayout(
                ayats,
                ResolveFont,
                fontSize,
                availableWidth,
                _glyphMeasurer);

        PART_HandwrittenCanvas.Width = availableWidth;
        PART_HandwrittenCanvas.Height = layout.TotalHeight;

        foreach (var line in layout.Lines)
        {
            foreach (var positioned in line.Glyphs)
            {
                var glyphVisual =
                    new TextBlock
                    {
                        Text = positioned.Glyph.Character.ToString(),
                        FontFamily = positioned.Glyph.Font,
                        FontSize = fontSize
                    };

                Canvas.SetLeft(glyphVisual, positioned.X);
                Canvas.SetTop(glyphVisual, positioned.Y);

                PART_HandwrittenCanvas.Children.Add(glyphVisual);
            }
        }
    }


    private void AttachContentFrameSizeHandlerOnce()
    {
        if (_contentFrameSizeHandlerAttached)
            return;

        _contentFrameSizeHandlerAttached = true;
        PART_ContentHost.SizeChanged += PART_ContentFrame_SizeChanged;
    }


    private void PART_ContentFrame_SizeChanged(
        object sender,
        SizeChangedEventArgs e)
    {
        PART_ContentHost.SizeChanged -= PART_ContentFrame_SizeChanged;
        _contentFrameSizeHandlerAttached = false;

        if (_currentPage is not null)
        {
            RenderPage(_currentPage);
        }
    }


    private static void ApplyAyatFont(
        Run run,
        Ayat ayat,
        IFontManager? manager)
    {
        if (manager is null)
            return;


        try
        {
            run.FontFamily =
                ayat.KodeFont == 0
                    ? manager.GetUnicodeFont()
                    : manager.GetSymbolFont(
                        ayat.KodeFont);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"[PAGE FONT] " +
                $"Fallback font: {ex.Message}");
        }
    }
}