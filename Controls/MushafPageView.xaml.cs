using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using System.Diagnostics;
using QuranKemenag.Models;
using QuranKemenag.Services.Font;

// ...

namespace QuranKemenag.Views.Controls;

public sealed partial class MushafPageView : UserControl
{
    public MushafPageView()
    {
        InitializeComponent();
        this.DataContextChanged += MushafPageView_DataContextChanged;
    }

    private void MushafPageView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (args.NewValue is not QuranPage page)
        {
            // Clear content when DataContext is not a QuranPage
            PART_RichText.Blocks.Clear();
            return;
        }

        Debug.WriteLine($"[PAGE VIEW] Page={page.PageNumber} AyatCount={page.Ayats?.Count ?? 0}");

        RenderPage(page);
    }

    private void RenderPage(QuranPage page)
    {
        if (page is null)
            return;

        PART_RichText.Blocks.Clear();

        var paragraph = new Paragraph
        {
            TextAlignment = TextAlignment.Right
        };

        var manager =
            App.Services?.GetService(typeof(IFontManager)) as IFontManager;

        var ayats = page.Ayats ?? System.Array.Empty<Ayat>();

        foreach (var ayat in ayats)
        {
            if (ayat is null)
                continue;

            // =====================================================
            // AYAT TEXT
            // =====================================================

            var arabicText = ayat.Teks?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(arabicText))
                continue;

            // Unicode RLM:
            // memastikan segmen ini diperlakukan sebagai RTL.
            const string RtlMark = "\u200F";

            var run = new Run
            {
                Text = RtlMark + arabicText + RtlMark,
                FontSize = PART_RichText.FontSize
            };

            // =====================================================
            // FONT AYAT
            // =====================================================

            try
            {
                if (manager is not null)
                {
                    run.FontFamily = ayat.KodeFont == 0
                        ? manager.GetUnicodeFont()
                        : manager.GetSymbolFont(ayat.KodeFont);
                }
            }
            catch
            {
                // fallback ke font default
            }

            paragraph.Inlines.Add(run);

            // =====================================================
            // NOMOR AYAT
            // =====================================================

            var ayatNumber =
                ayat.MushafAyatNumber ??
                ayat.AyatNumber.ToString();

            var numberRun = new Run
            {
                Text = $" {RtlMark}{ayatNumber}{RtlMark} ",
                FontSize = Math.Max(
                    16,
                    PART_RichText.FontSize * 0.4
                )
            };

            // =====================================================
            // FONT NOMOR AYAT
            // =====================================================

            try
            {
                if (manager is not null)
                {
                    numberRun.FontFamily = ayat.KodeFont == 0
                        ? manager.GetUnicodeFont()
                        : manager.GetSymbolFont(ayat.KodeFont);
                }
            }
            catch
            {
                // fallback
            }

            paragraph.Inlines.Add(numberRun);

            // Spasi antar ayat
            paragraph.Inlines.Add(
                new Run
                {
                    Text = " "
                });
        }

        PART_RichText.Blocks.Add(paragraph);
    }
}