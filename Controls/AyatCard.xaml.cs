using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Text;
using Microsoft.Extensions.DependencyInjection;

using Windows.ApplicationModel.DataTransfer;
//using Windows.UI.Text;

using QuranKemenag.Models;
using QuranKemenag.Services;
using QuranKemenag.Services.Font;

using QuranKemenag.Services.Page;




namespace QuranKemenag.Controls;

/// <summary>
/// Code-behind for AyatCard UserControl.
///
/// DataContext diharapkan berupa Models.Ayat.
///
/// AyatCard menangani:
/// - visibility terjemahan + footnote
/// - visibility bahasa isyarat
/// - ukuran font
/// - selected ayat / highlight
/// - font bahasa isyarat
/// </summary>
public sealed partial class AyatCard : UserControl
{
    // =========================================================
    // CACHE FONT ISYARAT
    // =========================================================
    //
    // Font Isyarat sama untuk seluruh AyatCard.
    //
    // Jangan ambil FontManager dari DI setiap ayat.
    // Kita ambil sekali lalu gunakan kembali.
    // =========================================================

    private static FontFamily? _isyaratFont;
    public event EventHandler<Ayat>? AyatSelected;
    private BookmarkService? _bookmarkService;


    private readonly IFontManager _fontManager = new FontManager();
    private readonly IGlyphMeasurer _handwrittenGlyphMeasurer =
    new TextBlockGlyphMeasurer();
    public FontFamily BismillahFont =>
    _fontManager.GetUnicodeFont();

    public AyatCard()
    {
        InitializeComponent();

        DataContextChanged += AyatCard_DataContextChanged;

        _bookmarkService = App.Services?.GetService<BookmarkService>();
    }


    // =========================================================
    // DATA CONTEXT
    // =========================================================

    private void AyatCard_DataContextChanged(
    FrameworkElement sender,
    DataContextChangedEventArgs args)
    {
        if (args.NewValue is not Ayat ayat)
            return;

        _ = LoadBookmarkStatusAsync(ayat);

        IsBookmarked = ayat.IsBookmarked;

        if (IsBookmarked)
        {
            PART_BookmarkButton.Content = "🔖 Bookmarked";
            ToolTipService.SetToolTip(
                PART_BookmarkButton,
                "Hapus Bookmark");

        }
        else
        {
            PART_BookmarkButton.Content = "🔖 Bookmark";

            ToolTipService.SetToolTip(
                PART_BookmarkButton,
                "Tambah Bookmark");
        }

        if (ayat.KodeFont == 0)
        {
            // Computer Font
            PART_TextTeks.Visibility =
                Visibility.Visible;

            PART_HandwrittenCanvas.Visibility =
                Visibility.Collapsed;

            PART_HandwrittenCanvas.Children.Clear();
        }
        else
        {
            // Handwritten Manuscript
            PART_TextTeks.Visibility =
                Visibility.Collapsed;

            PART_HandwrittenCanvas.Visibility =
                Visibility.Visible;

            RenderHandwritten(ayat);
        }

        // =====================================================
        // HEADER SURAH
        // =====================================================

        PART_SurahHeader.Visibility =
            ayat.IsSurahStart
                ? Visibility.Visible
                : Visibility.Collapsed;


        // =====================================================
        // BISMILLAH
        // =====================================================

        PART_Bismillah.Visibility =
            ayat.ShowBismillah
                ? Visibility.Visible
                : Visibility.Collapsed;


        // =====================================================
        // KODE LAMA
        // =====================================================

        UpdateTranslationVisibility(ayat);

        UpdateHighlight();

        // Nomor ayat hanya ditampilkan untuk mode Unicode.
        PART_AyatNumber.Visibility =
    ayat.KodeFont == 0
        ? Visibility.Visible
        : Visibility.Collapsed;

        if (ayat.KodeFont == 0)
        {
            DispatcherQueue.TryEnqueue(
                PositionAyatNumber);
        }
    }


  


    // =========================================================
    // SHOW TRANSLATION
    // =========================================================
    //
    // Mengontrol:
    // 1. Terjemahan
    // 2. Catatan kaki
    //
    // Jika ShowTranslation = false:
    // terjemahan DAN footnote disembunyikan.
    // =========================================================

    public static readonly DependencyProperty ShowTranslationProperty =
        DependencyProperty.Register(
            nameof(ShowTranslation),
            typeof(bool),
            typeof(AyatCard),
            new PropertyMetadata(
                true,
                OnDisplayOptionChanged));


    public bool ShowTranslation
    {
        get => (bool)GetValue(ShowTranslationProperty);

        set => SetValue(
            ShowTranslationProperty,
            value);
    }


    // =========================================================
    // SHOW ISYARAT
    // =========================================================

    public static readonly DependencyProperty ShowIsyaratProperty =
        DependencyProperty.Register(
            nameof(ShowIsyarat),
            typeof(bool),
            typeof(AyatCard),
            new PropertyMetadata(
                true,
                OnDisplayOptionChanged));


    public bool ShowIsyarat
    {
        get => (bool)GetValue(ShowIsyaratProperty);

        set => SetValue(
            ShowIsyaratProperty,
            value);
    }


    // =========================================================
    // ARABIC FONT SIZE
    // =========================================================

    public static readonly DependencyProperty ArabicFontSizeProperty =
        DependencyProperty.Register(
            nameof(ArabicFontSize),
            typeof(double),
            typeof(AyatCard),
            new PropertyMetadata(42.0));


    public double ArabicFontSize
    {
        get => (double)GetValue(ArabicFontSizeProperty);

        set => SetValue(
            ArabicFontSizeProperty,
            value);
    }


    // =========================================================
    // TRANSLATION FONT SIZE
    // =========================================================

    public static readonly DependencyProperty TranslationFontSizeProperty =
        DependencyProperty.Register(
            nameof(TranslationFontSize),
            typeof(double),
            typeof(AyatCard),
            new PropertyMetadata(18.0));


    public double TranslationFontSize
    {
        get => (double)GetValue(TranslationFontSizeProperty);

        set => SetValue(
            TranslationFontSizeProperty,
            value);
    }


    // =========================================================
    // ISYARAT FONT SIZE
    // =========================================================

    public static readonly DependencyProperty IsyaratFontSizeProperty =
        DependencyProperty.Register(
            nameof(IsyaratFontSize),
            typeof(double),
            typeof(AyatCard),
            new PropertyMetadata(22.0));


    public double IsyaratFontSize
    {
        get => (double)GetValue(IsyaratFontSizeProperty);

        set => SetValue(
            IsyaratFontSizeProperty,
            value);
    }


    // =========================================================
    // DISPLAY OPTION CHANGED
    // =========================================================

    private static void OnDisplayOptionChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        if (d is not AyatCard card)
            return;

        if (card.DataContext is not Ayat ayat)
            return;

        card.UpdateTranslationVisibility(ayat);
    }


    // =========================================================
    // SELECTED AYAT
    // =========================================================

    /// <summary>
    /// Ayat yang sedang dipilih oleh View.
    /// Digunakan untuk menentukan highlight pada kartu ayat.
    /// </summary>

    public object? SelectedAyat
    {
        get => GetValue(SelectedAyatProperty);

        set => SetValue(
            SelectedAyatProperty,
            value);
    }


    public static readonly DependencyProperty SelectedAyatProperty =
        DependencyProperty.Register(
            nameof(SelectedAyat),
            typeof(object),
            typeof(AyatCard),
            new PropertyMetadata(
                null,
                OnSelectedAyatChanged));


    private static void OnSelectedAyatChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        if (d is AyatCard card)
        {
            card.UpdateHighlight();
        }
    }

    private void PositionAyatNumber()
    {
        if (DataContext is not Ayat ayat)
            return;

        if (ayat.KodeFont != 0)
        {
            PART_AyatNumber.Visibility =
                Visibility.Collapsed;

            return;
        }

        PART_AyatNumber.Visibility =
            Visibility.Visible;

        // Letakkan nomor di baris paling bawah.
        double top =
            PART_AyatNumberCanvas.ActualHeight
            - PART_AyatNumber.Height
            - 6;

        if (top < 0)
            top = 0;

        Canvas.SetTop(
            PART_AyatNumber,
            top);
    }


    // =========================================================
    // UPDATE VISIBILITY
    // =========================================================

    private void UpdateTranslationVisibility(Ayat ayat)
    {
        // =====================================================
        // TERJEMAHAN
        // =====================================================

        PART_TranslationContainer.Visibility =
            ShowTranslation &&
            !string.IsNullOrWhiteSpace(
                ayat.Terjemahan)
                ? Visibility.Visible
                : Visibility.Collapsed;


        // =====================================================
        // CATATAN KAKI
        //
        // Footnote mengikuti terjemahan.
        //
        // Jika terjemahan OFF:
        // footnote juga OFF.
        // =====================================================

        PART_FootnoteContainer.Visibility =
            ShowTranslation &&
            !string.IsNullOrWhiteSpace(
                ayat.TextFn)
                ? Visibility.Visible
                : Visibility.Collapsed;


        // =====================================================
        // BAHASA ISYARAT
        //
        // Independen dari terjemahan.
        // =====================================================

        bool showIsyarat =
            ShowIsyarat &&
            !string.IsNullOrWhiteSpace(
                ayat.Isyarat);


        PART_IsyaratContainer.Visibility =
            showIsyarat
                ? Visibility.Visible
                : Visibility.Collapsed;


        // =====================================================
        // FONT BAHASA ISYARAT
        // =====================================================
        //
        // Hanya lakukan jika Isyarat memang ditampilkan.
        // Font disimpan dalam static cache.
        // =====================================================

        if (showIsyarat)
        {
            ApplyIsyaratFont();
        }
    }


    // =========================================================
    // APPLY ISYARAT FONT
    // =========================================================

    private void ApplyIsyaratFont()
    {
        // -----------------------------------------------------
        // Jika font sudah pernah diambil,
        // langsung gunakan.
        // -----------------------------------------------------

        if (_isyaratFont is not null)
        {
            PART_Isyarat.FontFamily =
                _isyaratFont;

            return;
        }


        // -----------------------------------------------------
        // Ambil FontManager hanya sekali.
        // -----------------------------------------------------

        var manager =
            App.Services.GetService(
                typeof(IFontManager))
            as IFontManager;


        if (manager is null)
            return;


        // -----------------------------------------------------
        // Ambil font dari FontManager.
        // -----------------------------------------------------

        var font =
            manager.GetIsyaratFont();


        if (font is null)
            return;


        // -----------------------------------------------------
        // Simpan ke cache.
        // -----------------------------------------------------

        _isyaratFont =
            font;


        // -----------------------------------------------------
        // Terapkan ke TextBlock.
        // -----------------------------------------------------

        PART_Isyarat.FontFamily =
            _isyaratFont;
    }


    // =========================================================
    // HIGHLIGHT AYAT
    // =========================================================

    private void UpdateHighlight()
    {
        try
        {
            var currentId =
                GetAyatId(DataContext);

            var selectedId =
                GetAyatId(SelectedAyat);


            bool isSelected =
                currentId.HasValue &&
                selectedId.HasValue &&
                currentId.Value ==
                selectedId.Value;


            if (PART_Border is null)
                return;


            var resources =
                Application.Current?.Resources;


            if (isSelected)
            {
                // =================================================
                // HIGHLIGHT BACKGROUND
                // =================================================

                var bg =
                    resources?[
                        "LpmqGreenSoftBrush"]
                    as Brush
                    ??
                    new SolidColorBrush(
                        Microsoft.UI.Colors
                            .LightGoldenrodYellow);


                // =================================================
                // HIGHLIGHT BORDER
                // =================================================

                var border =
                    resources?[
                        "LpmqGreenBrush"]
                    as Brush
                    ??
                    new SolidColorBrush(
                        Microsoft.UI.Colors
                            .Goldenrod);


                // =================================================
                // HIGHLIGHT THICKNESS
                // =================================================

                var thickness =
                    resources?[
                        "HighlightBorderThickness"]
                    as Thickness?
                    ??
                    new Thickness(2);


                PART_Border.Background =
                    bg;

                PART_Border.BorderBrush =
                    border;

                PART_Border.BorderThickness =
                    thickness;
            }
            else
            {
                // =================================================
                // NORMAL BACKGROUND
                // =================================================

                var bg =
                    resources?[
                        "NeutralSurfaceBrush"]
                    as Brush
                    ??
                    new SolidColorBrush(
                        Microsoft.UI.Colors
                            .Transparent);


                // =================================================
                // NORMAL BORDER
                // =================================================

                var border =
                    resources?[
                        "NeutralBorderBrush"]
                    as Brush
                    ??
                    new SolidColorBrush(
                        Microsoft.UI.Colors
                            .Transparent);


                var thickness =
                    resources?[
                        "CardBorderStyle"] == null
                        ? new Thickness(1)
                        : new Thickness(1);


                PART_Border.Background =
                    bg;

                PART_Border.BorderBrush =
                    border;

                PART_Border.BorderThickness =
                    thickness;
            }
        }
        catch
        {
            // Jangan biarkan error highlight
            // mengganggu Reader.
        }
    }


    // =========================================================
    // AYAT ID
    // =========================================================

    private static int? GetAyatId(
        object? item)
    {
        return item switch
        {
            Ayat ayat =>
                ayat.Id,

            AyatSymbol symbol =>
                symbol.Id,

            _ => null
        };
    }

    private void PART_Border_Tapped(
    object sender,
    Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        if (DataContext is not Ayat ayat)
            return;

        AyatSelected?.Invoke(
        this,
        ayat);
    }

    // =========================================================
    // COPY AYAT
    // =========================================================
    //
    // Prinsip:
    // 1. Sumber hanya dari satu object Ayat.
    // 2. Arab tidak pernah diambil dari visual UI.
    // 3. Nomor ayat ikut disalin.
    // 4. Terjemahan + footnote mengikuti ShowTranslation.
    // 5. Isyarat mengikuti ShowIsyarat.
    // 6. Clipboard menyediakan Plain Text + RTF.
    // 7. Word menggunakan font sesuai KodeFont.
    //
    // =========================================================


    private static string GetArabicFontName(
    Ayat ayat)
    {
        // =====================================================
        // UNICODE
        // =====================================================

        if (ayat.KodeFont == 0)
        {
            return "LPMQ Isep Misbah";
        }


        // =====================================================
        // HANDWRITTEN / SYMBOL
        // =====================================================

        return $"Lpmq_Hafs_{ayat.KodeFont:00}";
    }


    // =========================================================
    // RTF ESCAPE
    // =========================================================

    private static string EscapeRtf(
        string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;


        var builder =
            new StringBuilder();


        foreach (char c in text)
        {
            switch (c)
            {
                case '\\':
                    builder.Append(@"\\");
                    break;

                case '{':
                    builder.Append(@"\{");
                    break;

                case '}':
                    builder.Append(@"\}");
                    break;

                case '\r':
                    break;

                case '\n':
                    builder.Append(@"\line ");
                    break;

                default:

                    // ASCII biasa
                    if (c >= 32 && c <= 126)
                    {
                        builder.Append(c);
                    }
                    else
                    {
                        // RTF menggunakan signed 16-bit integer
                        short value =
                            unchecked((short)c);

                        builder.Append(@"\u");
                        builder.Append(value);
                        builder.Append("?");
                    }

                    break;
            }
        }


        return builder.ToString();
    }


    // =========================================================
    // PLAIN TEXT
    // =========================================================


    private static string NormalizeMushafAyatNumber(
     string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        string text =
            value.Trim();


        // -----------------------------------------------------
        // Hapus RTF control word jika data lama masih memilikinya
        // -----------------------------------------------------

        text =
            System.Text.RegularExpressions.Regex.Replace(
                text,
                @"\\f\d+\s*",
                string.Empty);


        // -----------------------------------------------------
        // Konversi \uNNNN? menjadi Unicode
        // -----------------------------------------------------

        text =
            System.Text.RegularExpressions.Regex.Replace(
                text,
                @"\\u(-?\d+)\?",
                match =>
                {
                    if (!int.TryParse(
                        match.Groups[1].Value,
                        out int code))
                    {
                        return string.Empty;
                    }

                    if (code < 0)
                        code += 65536;

                    return ((char)code).ToString();
                });


        return text.Trim();
    }

    private static string BuildPlainCopyText(
    Ayat ayat,
    bool showTranslation,
    bool showIsyarat)
    {
        var builder = new StringBuilder();

        // =====================================================
        // 1. AYAT
        // =====================================================

        if (!string.IsNullOrWhiteSpace(ayat.Teks))
        {
            builder.Append(
                ayat.Teks.Trim());
        }


        // =====================================================
        // 2. NOMOR AYAT
        // =====================================================
        //
        // HANYA Unicode.
        // Handwritten tidak memiliki nomor ayat.
        //

        if (ayat.KodeFont == 0)
        {
            string number =
                NormalizeMushafAyatNumber(
                    ayat.MushafAyatNumber);

            if (!string.IsNullOrWhiteSpace(
                number))
            {
                builder.Append(' ');

                builder.Append(
                    number);
            }
        }


        // =====================================================
        // 3. TERJEMAHAN
        // =====================================================

        if (showTranslation &&
            !string.IsNullOrWhiteSpace(
                ayat.Terjemahan))
        {
            builder.AppendLine();
            builder.AppendLine();

            builder.Append(
                ayat.Terjemahan.Trim());
        }


        // =====================================================
        // 4. FOOTNOTE
        // =====================================================

        if (showTranslation &&
            !string.IsNullOrWhiteSpace(
                ayat.TextFn))
        {
            builder.AppendLine();
            builder.AppendLine();

            builder.Append(
                $"Catatan Kaki: {ayat.TextFn.Trim()}");
        }


        // =====================================================
        // 5. BAHASA ISYARAT
        // =====================================================

        if (showIsyarat &&
            !string.IsNullOrWhiteSpace(
                ayat.Isyarat))
        {
            builder.AppendLine();
            builder.AppendLine();

            builder.Append(
                ayat.Isyarat.Trim());
        }


        return builder
            .ToString()
            .Trim();
    }


    // =========================================================
    // RTF
    // =========================================================

    private static string BuildRtf(
    Ayat ayat,
    bool showTranslation,
    bool showIsyarat)
    {
        var builder = new StringBuilder();

        // =====================================================
        // FONT
        // =====================================================

        string arabicFont =
            GetArabicFontName(ayat);


        // =====================================================
        // RTF HEADER
        // =====================================================

        builder.Append(
            @"{\rtf1\ansi\ansicpg1252\deff0\uc1");

        builder.Append(
            @"\viewkind4");


        // =====================================================
        // FONT TABLE
        // =====================================================

        builder.Append(
            @"{\fonttbl");


        // -----------------------------------------------------
        // f0 = FONT QURAN
        // -----------------------------------------------------

        builder.Append(
            $@"{{\f0\fnil\fcharset178 {EscapeRtf(arabicFont)};}}");


        // -----------------------------------------------------
        // f1 = FONT LATIN
        // -----------------------------------------------------

        builder.Append(
            @"{\f1\fnil\fcharset0 Aptos;}");


        // -----------------------------------------------------
        // f2 = FONT ISYARAT
        // -----------------------------------------------------

        builder.Append(
            @"{\f2\fnil\fcharset0 LPMQ MSI ISYARAT;}");


        builder.Append("}");


        // =====================================================
        // DEFAULT DOCUMENT
        // =====================================================

        builder.Append(
            @"\paperw11907\paperh16840");

        builder.Append(
            @"\margl1440\margr1440");

        builder.Append(
            @"\margt1440\margb1440");


        // =====================================================
        // AYAT ARAB
        // =====================================================

        builder.Append(
            @"\pard\rtlpar\qr");

        builder.Append(
            @"\rtlch\f0\af0\fs32");


        if (!string.IsNullOrWhiteSpace(
            ayat.Teks))
        {
            builder.Append(
                EscapeRtf(
                    ayat.Teks.Trim()));
        }


        // =====================================================
        // NOMOR AYAT
        // =====================================================
        //
        // HANYA Unicode.
        // Handwritten tidak mempunyai nomor ayat.
        //

        if (ayat.KodeFont == 0 &&
            !string.IsNullOrWhiteSpace(
                ayat.MushafAyatNumber))
        {
            string mushafNumber =
                NormalizeMushafAyatNumber(
                    ayat.MushafAyatNumber);

            if (!string.IsNullOrWhiteSpace(
                mushafNumber))
            {
                builder.Append(' ');

                // Nomor menggunakan font Latin
                builder.Append(
                    @"\ltrch\f1\af1\fs24 ");

                builder.Append(
                    EscapeRtf(
                        mushafNumber));
            }
        }


        builder.Append(
            @"\par");


        // =====================================================
        // TERJEMAHAN
        // =====================================================

        if (showTranslation &&
            !string.IsNullOrWhiteSpace(
                ayat.Terjemahan))
        {
            builder.Append(
                @"\pard\ltrpar\ql");

            builder.Append(
                @"\ltrch\f1\af1\fs24 ");

            builder.Append(
                EscapeRtf(
                    ayat.Terjemahan.Trim()));

            builder.Append(
                @"\par");
        }


        // =====================================================
        // FOOTNOTE
        // =====================================================

        if (showTranslation &&
            !string.IsNullOrWhiteSpace(
                ayat.TextFn))
        {
            builder.Append(
                @"\pard\ltrpar\ql");

            builder.Append(
                @"\ltrch\f1\af1\fs20 ");

            builder.Append(
                EscapeRtf(
                    $"Catatan Kaki: {ayat.TextFn.Trim()}"));

            builder.Append(
                @"\par");
        }


        // =====================================================
        // BAHASA ISYARAT
        // =====================================================

        if (showIsyarat &&
            !string.IsNullOrWhiteSpace(
                ayat.Isyarat))
        {
            builder.Append(
                @"\pard\rtlpar\qr");

            builder.Append(
                @"\rtlch\f2\af2\fs24 ");

            builder.Append(
                EscapeRtf(
                    ayat.Isyarat.Trim()));

            builder.Append(
                @"\par");
        }


        // =====================================================
        // END RTF
        // =====================================================

        builder.Append("}");

        return builder.ToString();
    }


    // =========================================================
    // COPY BUTTON
    // =========================================================

    private void PART_CopyButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        if (DataContext is not Ayat ayat)
            return;


        // =====================================================
        // PLAIN TEXT
        // =====================================================

        string plainText =
            BuildPlainCopyText(
                ayat,
                ShowTranslation,
                ShowIsyarat);

        if (string.IsNullOrWhiteSpace(
            plainText))
        {
            return;
        }


        // =====================================================
        // RTF (use the hand-crafted RTF generator so Word
        // receives a font table that matches the app fonts)
        // =====================================================

        string rtf =
            BuildRtf(
                ayat,
                ShowTranslation,
                ShowIsyarat);


        // =====================================================
        // CLIPBOARD
        // =====================================================

        var package =
            new DataPackage();


        package.SetText(
            plainText);


        package.SetRtf(
            rtf);


        Clipboard.SetContent(
            package);
    }

    // =========================================================
    // BOOKMARK - TAHAP 1
    // =========================================================

    public static readonly DependencyProperty IsBookmarkedProperty =
        DependencyProperty.Register(
            nameof(IsBookmarked),
            typeof(bool),
            typeof(AyatCard),
            new PropertyMetadata(false));

    public bool IsBookmarked
    {
        get => (bool)GetValue(IsBookmarkedProperty);

        set => SetValue(
            IsBookmarkedProperty,
            value);
    }


    private async void PART_BookmarkButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        if (DataContext is not Ayat ayat)
            return;

        if (_bookmarkService is null)
            return;

        // ============================================================
        // JIKA SUDAH BOOKMARK → HAPUS LANGSUNG
        // ============================================================

        if (ayat.IsBookmarked)
        {
            var existing = await _bookmarkService.GetAsync(
                ayat.Surah,
                ayat.AyatNumber);

            if (existing is not null)
            {
                await _bookmarkService.DeleteAsync(existing.Id);
            }

            ayat.IsBookmarked = false;
            IsBookmarked = false;

            PART_BookmarkButton.Content = "🔖 Bookmark";

            ToolTipService.SetToolTip(
                PART_BookmarkButton,
                "Tambah Bookmark");

            return;
        }

        // ============================================================
        // JIKA BELUM BOOKMARK → TAMPILKAN DIALOG CATATAN
        // ============================================================

        if (XamlRoot is null)
            return;

        var noteTextBox = new TextBox
        {
            PlaceholderText = "Tulis catatan...",
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 100
        };

        var contentPanel = new StackPanel
        {
            Spacing = 8
        };

        contentPanel.Children.Add(
            new TextBlock
            {
                Text = $"Surah {ayat.Surah} • Ayat {ayat.AyatNumber}",
                FontSize = 14
            });

        contentPanel.Children.Add(
            new TextBlock
            {
                Text = "Catatan",
                FontSize = 13
            });

        contentPanel.Children.Add(noteTextBox);

        var dialog = new ContentDialog
        {
            Title = "Tambah Bookmark",
            Content = contentPanel,

            PrimaryButtonText = "Simpan",
            CloseButtonText = "Batal",

            DefaultButton = ContentDialogButton.Primary,

            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result != ContentDialogResult.Primary)
            return;

        // ============================================================
        // SIMPAN BOOKMARK
        // ============================================================

        string? note = noteTextBox.Text?.Trim();

        await _bookmarkService.AddAsync(
            ayat.Surah,
            ayat.AyatNumber,
            note);

        ayat.IsBookmarked = true;
        IsBookmarked = true;

        PART_BookmarkButton.Content = "🔖 Bookmarked";

        ToolTipService.SetToolTip(
            PART_BookmarkButton,
            "Hapus Bookmark");
    }




    private static string EscapeHtml(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return System.Net.WebUtility.HtmlEncode(text);
    }

    private static string BuildHtml(
    Ayat ayat,
    bool showTranslation,
    bool showIsyarat)
    {
        var builder = new StringBuilder();

        string arabicFont =
            GetArabicFontName(ayat);

        // =====================================================
        // HTML
        // =====================================================

        builder.Append("""
<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8">
<style>
body {
    margin: 0;
    padding: 0;
}

.quran {
    direction: rtl;
    text-align: right;
    font-size: 32pt;
    line-height: 1.8;
}

.translation {
    direction: ltr;
    text-align: left;
    font-family: Aptos, Arial, sans-serif;
    font-size: 12pt;
    line-height: 1.5;
    margin-top: 12pt;
}

.footnote {
    direction: ltr;
    text-align: left;
    font-family: Aptos, Arial, sans-serif;
    font-size: 10pt;
    margin-top: 8pt;
}

.isyarat {
    direction: rtl;
    text-align: right;
    font-family: 'LPMQ MSI ISYARAT';
    font-size: 18pt;
    margin-top: 12pt;
}
</style>
</head>
<body>
""");


        // =====================================================
        // AYAT
        // =====================================================

        builder.Append(
            $"""
<div class="quran"
     style="font-family:'{EscapeHtml(arabicFont)}';">
""");


        if (!string.IsNullOrWhiteSpace(
            ayat.Teks))
        {
            builder.Append(
                EscapeHtml(
                    ayat.Teks.Trim()));
        }


        // =====================================================
        // NOMOR AYAT
        // =====================================================

        // Hanya Unicode
        if (ayat.KodeFont == 0 &&
    !string.IsNullOrWhiteSpace(
        ayat.MushafAyatNumber))
        {
            string mushafNumber =
                NormalizeMushafAyatNumber(
                    ayat.MushafAyatNumber);

            builder.Append(" ");

            builder.Append(
                $"""
<span style="
    font-family:Aptos,Arial,sans-serif;
">
{EscapeHtml(mushafNumber)}
</span>
""");
        }


        builder.Append("</div>");


        // =====================================================
        // TERJEMAHAN
        // =====================================================

        if (showTranslation &&
            !string.IsNullOrWhiteSpace(
                ayat.Terjemahan))
        {
            builder.Append(
                $"""
<div class="translation">
{EscapeHtml(ayat.Terjemahan.Trim())}
</div>
""");
        }


        // =====================================================
        // FOOTNOTE
        // =====================================================

        if (showTranslation &&
            !string.IsNullOrWhiteSpace(
                ayat.TextFn))
        {
            builder.Append(
                $"""
<div class="footnote">
Catatan Kaki: {EscapeHtml(ayat.TextFn.Trim())}
</div>
""");
        }


        // =====================================================
        // ISYARAT
        // =====================================================

        if (showIsyarat &&
            !string.IsNullOrWhiteSpace(
                ayat.Isyarat))
        {
            builder.Append(
                $"""
<div class="isyarat">
{EscapeHtml(ayat.Isyarat.Trim())}
</div>
""");
        }


        builder.Append("""
</body>
</html>
""");

        return builder.ToString();
    }

    private static string BuildWordRtf(
    Ayat ayat,
    bool showTranslation,
    bool showIsyarat)
    {
        // =====================================================
        // RichEditBox
        // =====================================================

        var editor =
            new RichEditBox();

        editor.Visibility =
            Visibility.Collapsed;

        var document =
            editor.Document;


        // =====================================================
        // PLAIN CONTENT
        // =====================================================

        var parts =
            new List<string>();


        // =====================================================
        // AYAT
        // =====================================================

        string arabicText =
            ayat.Teks?.Trim()
            ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(
            arabicText))
        {
            parts.Add(
                arabicText);
        }


        // =====================================================
        // NOMOR AYAT
        // =====================================================

        //
        // HANYA UNICODE
        //
        // Handwritten tidak mempunyai nomor.
        //

        if (ayat.KodeFont == 0)
        {
            string number =
                NormalizeMushafAyatNumber(
                    ayat.MushafAyatNumber);

            if (!string.IsNullOrWhiteSpace(
                number))
            {
                parts.Add(
                    number);
            }
        }


        // =====================================================
        // TERJEMAHAN
        // =====================================================

        if (showTranslation &&
            !string.IsNullOrWhiteSpace(
                ayat.Terjemahan))
        {
            parts.Add(
                ayat.Terjemahan.Trim());
        }


        // =====================================================
        // FOOTNOTE
        // =====================================================

        if (showTranslation &&
            !string.IsNullOrWhiteSpace(
                ayat.TextFn))
        {
            parts.Add(
                $"Catatan Kaki: {ayat.TextFn.Trim()}");
        }


        // =====================================================
        // ISYARAT
        // =====================================================

        if (showIsyarat &&
            !string.IsNullOrWhiteSpace(
                ayat.Isyarat))
        {
            parts.Add(
                ayat.Isyarat.Trim());
        }


        // =====================================================
        // SATUKAN TEKS
        // =====================================================

        string fullText =
            string.Join(
                "\r\n\r\n",
                parts);


        document.SetText(
    Microsoft.UI.Text.TextSetOptions.None,
    fullText);


        // =====================================================
        // FONT AYAT
        // =====================================================

        int arabicLength =
            arabicText.Length;

        if (arabicLength > 0)
        {
            var arabicRange =
                document.GetRange(
                    0,
                    arabicLength);

            arabicRange.CharacterFormat.Name =
                GetArabicFontName(ayat);

            arabicRange.CharacterFormat.Size =
                16;
        }


        // =====================================================
        // NOMOR AYAT
        // =====================================================

        int currentPosition =
            arabicLength;


        if (ayat.KodeFont == 0)
        {
            string number =
                NormalizeMushafAyatNumber(
                    ayat.MushafAyatNumber);

            if (!string.IsNullOrWhiteSpace(
                number))
            {
                currentPosition += 2;

                var numberRange =
                    document.GetRange(
                        currentPosition,
                        currentPosition +
                        number.Length);

                numberRange.CharacterFormat.Name =
                    "Aptos";

                numberRange.CharacterFormat.Size =
                    12;

                currentPosition +=
                    number.Length;
            }
        }


        // =====================================================
        // TERJEMAHAN
        // =====================================================

        if (showTranslation &&
            !string.IsNullOrWhiteSpace(
                ayat.Terjemahan))
        {
            currentPosition += 2;

            int start =
                currentPosition;

            int length =
                ayat.Terjemahan.Trim().Length;

            var translationRange =
                document.GetRange(
                    start,
                    start + length);

            translationRange.CharacterFormat.Name =
                "Aptos";

            translationRange.CharacterFormat.Size =
                12;

            currentPosition +=
                length;
        }


        // =====================================================
        // FOOTNOTE
        // =====================================================

        if (showTranslation &&
            !string.IsNullOrWhiteSpace(
                ayat.TextFn))
        {
            string footnote =
                $"Catatan Kaki: {ayat.TextFn.Trim()}";

            currentPosition += 2;

            int start =
                currentPosition;

            var footnoteRange =
                document.GetRange(
                    start,
                    start + footnote.Length);

            footnoteRange.CharacterFormat.Name =
                "Aptos";

            footnoteRange.CharacterFormat.Size =
                10;

            currentPosition +=
                footnote.Length;
        }


        // =====================================================
        // ISYARAT
        // =====================================================

        if (showIsyarat &&
            !string.IsNullOrWhiteSpace(
                ayat.Isyarat))
        {
            currentPosition += 2;

            int start =
                currentPosition;

            int length =
                ayat.Isyarat.Trim().Length;

            var isyaratRange =
                document.GetRange(
                    start,
                    start + length);

            isyaratRange.CharacterFormat.Name =
                "LPMQ MSI ISYARAT";

            isyaratRange.CharacterFormat.Size =
                14;
        }


        // =====================================================
        // AMBIL RTF
        // =====================================================

        document.GetText(
            TextGetOptions.FormatRtf,
            out string rtf);

        return rtf;
    }

    private void PART_AyatNumberCanvas_SizeChanged(
    object sender,
    SizeChangedEventArgs e)
    {
        if (DataContext is Ayat ayat &&
            ayat.KodeFont == 0)
        {
            PositionAyatNumber();
        }
    }

    private void RenderHandwritten(Ayat ayat)
    {
        if (PART_HandwrittenCanvas is null)
            return;
        // Compute available width from the actual area allocated for the ayat text.
        // Do not use PART_HandwrittenCanvas.ActualWidth because the canvas may
        // have retained a previously larger width. Use PART_AyatArea (the
        // containing element) and subtract the ayat number column width to get
        // the true text area width.
        double areaWidth = PART_AyatArea?.ActualWidth ?? 0;
        double numberColWidth = PART_AyatNumberCanvas?.ActualWidth ?? 0;

        double availableWidth = Math.Max(0, areaWidth - numberColWidth);

        Debug.WriteLine($"[HANDWRITTEN RESPONSIVE] PART_AyatArea.ActualWidth={PART_AyatArea?.ActualWidth:F1}, PART_TextTeks.ActualWidth={PART_TextTeks?.ActualWidth:F1}, PART_HandwrittenCanvas.ActualWidth={PART_HandwrittenCanvas?.ActualWidth:F1}, availableWidth={availableWidth:F1}, numberColWidth={PART_AyatNumberCanvas?.ActualWidth:F1}");

        if (availableWidth <= 0)
            return;

        PART_HandwrittenCanvas.Children.Clear();

        var layout =
            HandwrittenLayoutEngine.BuildLayout(
                new[] { ayat },
                kodeFont =>
                    _fontManager.GetSymbolFont(kodeFont),
                ArabicFontSize,
                availableWidth,
                _handwrittenGlyphMeasurer);

        // Ensure the canvas width matches the computed available text width so it
        // no longer retains a stale larger width and pushes glyphs outside the
        // visible region.
        PART_HandwrittenCanvas.Width = availableWidth;

        PART_HandwrittenCanvas.Height =
            layout.TotalHeight;

        foreach (var line in layout.Lines)
        {
            foreach (var positioned in line.Glyphs)
            {
                var glyphVisual =
                    new TextBlock
                    {
                        Text =
                            positioned.Glyph.Character.ToString(),

                        FontFamily =
                            positioned.Glyph.Font,

                        FontSize =
                            ArabicFontSize,

                        IsHitTestVisible =
                            false
                    };

                Canvas.SetLeft(
                    glyphVisual,
                    positioned.X);

                Canvas.SetTop(
                    glyphVisual,
                    positioned.Y);

                PART_HandwrittenCanvas.Children.Add(
                    glyphVisual);
            }
        }

        // Diagnostic: report layout extents
        try
        {
            double rightmost = 0;
            foreach (var line in layout.Lines)
            {
                if (line.Glyphs.Count == 0)
                    continue;

                var last = line.Glyphs[line.Glyphs.Count - 1];
                var extent = last.X + last.Glyph.Width;
                rightmost = Math.Max(rightmost, extent);
            }

            Debug.WriteLine($"[HANDWRITTEN RESPONSIVE] layout.TotalHeight={layout.TotalHeight:F1}, lines={layout.Lines.Count}, rightmostExtent={rightmost:F1}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[HANDWRITTEN RESPONSIVE] diagnostic error: " + ex.ToString());
        }
    }

    private void PART_AyatArea_SizeChanged(
    object sender,
    SizeChangedEventArgs e)
    {
        if (DataContext is not Ayat ayat)
            return;

        if (ayat.KodeFont == 0)
            return;

        if (e.NewSize.Width <= 0)
            return;

        Debug.WriteLine($"[HANDWRITTEN RESPONSIVE] PART_AyatArea_SizeChanged NewWidth={e.NewSize.Width:F1} OldWidth={e.PreviousSize.Width:F1}");

        DispatcherQueue.TryEnqueue(() =>
        {
            if (DataContext is not Ayat currentAyat)
                return;

            if (currentAyat.KodeFont == 0)
                return;

            RenderHandwritten(currentAyat);
        });
    }

    private async Task LoadBookmarkStatusAsync(Ayat ayat)
    {
        if (_bookmarkService is null)
            return;

        try
        {
            var bookmark = await _bookmarkService.GetAsync(
                ayat.Surah,
                ayat.AyatNumber);

            ayat.IsBookmarked = bookmark is not null;
            IsBookmarked = ayat.IsBookmarked;

            if (IsBookmarked)
            {
                PART_BookmarkButton.Content = "🔖 Bookmarked";
                ToolTipService.SetToolTip(
                    PART_BookmarkButton,
                    "Hapus Bookmark");
            }
            else
            {
                PART_BookmarkButton.Content = "🔖 Bookmark";
                ToolTipService.SetToolTip(
                    PART_BookmarkButton,
                    "Tambah Bookmark");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"[BOOKMARK] Gagal memuat status Surah={ayat.Surah}, Ayat={ayat.AyatNumber}: {ex}");
        }
    }

}