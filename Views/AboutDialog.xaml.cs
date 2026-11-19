using System;
using System.Reflection;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;

using QuranKemenag.Constants;

namespace QuranKemenag.Views;

/// <summary>
/// Isi dialog "Tentang Aplikasi".
///
/// TUGAS CLASS INI:
///   Membaca daftar blok dari <see cref="AboutInfo"/> lalu membuat
///   elemen tampilannya (judul, paragraf, butir, susunan tim) ke dalam
///   BlocksPanel di AboutDialog.xaml.
///
/// YANG PERLU DIEDIT UNTUK MENGUBAH TEKS:
///   Bukan file ini, melainkan Constants/AboutInfo.cs.
///
/// YANG BISA DIEDIT DI FILE INI:
///   Hanya bagian "PENGATURAN TAMPILAN" di bawah (rata teks, ukuran huruf).
/// </summary>
public sealed partial class AboutDialog : UserControl
{
    // =============================================================
    // PENGATURAN TAMPILAN
    // =============================================================

    /// <summary>
    /// Rata teks JUDUL. Center = rata tengah.
    /// Pilihan: Left, Center, Right, Justify.
    /// </summary>
    private const TextAlignment HeadingTextAlignment =
        TextAlignment.Center;

    /// <summary>
    /// Rata teks PARAGRAF. Justify = rata kiri-kanan (rapi seperti
    /// buku). Ganti ke Center jika ingin seluruh paragraf rata tengah,
    /// atau Left untuk rata kiri.
    /// </summary>
    private const TextAlignment ParagraphTextAlignment =
        TextAlignment.Justify;

    /// <summary>
    /// Rata teks DAFTAR BUTIR. Left disarankan agar butir mudah dibaca.
    /// </summary>
    private const TextAlignment BulletTextAlignment =
        TextAlignment.Left;

    /// <summary>
    /// Rata teks SUSUNAN TIM (jabatan dan nama). Center = rata tengah.
    /// </summary>
    private const TextAlignment CreditTextAlignment =
        TextAlignment.Center;

    /// <summary>Ukuran huruf judul.</summary>
    private const double HeadingFontSize = 17;

    /// <summary>Ukuran huruf paragraf, butir, dan susunan tim.</summary>
    private const double BodyFontSize = 14;

    /// <summary>Tinggi baris paragraf (jarak antar baris).</summary>
    private const double BodyLineHeight = 22;

    /// <summary>
    /// Jorokan butir anak (BulletGroup) dari tepi kiri, dalam piksel.
    /// </summary>
    private const double SubBulletIndent = 22;

    /// <summary>Tanda untuk butir induk dan butir biasa.</summary>
    private const string BulletMarker = "•";

    /// <summary>Tanda untuk butir anak (tingkat kedua).</summary>
    private const string SubBulletMarker = "◦";


    // =============================================================
    // PEMBUATAN ISI
    // =============================================================

    public AboutDialog()
    {
        this.InitializeComponent();

        FillContent();
    }

    /// <summary>
    /// Mengisi header, isi, dan hak cipta.
    /// </summary>
    private void FillContent()
    {
        // ---------- HEADER ----------

        AppNameText.Text =
            AboutInfo.AppName;

        VersionText.Text =
            $"Versi {GetAppVersion()}";


        // ---------- ISI ----------

        BuildBlocks();


        // ---------- HAK CIPTA ----------

        if (string.IsNullOrWhiteSpace(AboutInfo.Copyright))
        {
            CopyrightText.Visibility =
                Visibility.Collapsed;
        }
        else
        {
            CopyrightText.Text =
                AboutInfo.Copyright;
        }
    }

    /// <summary>
    /// Membuat elemen tampilan untuk setiap blok di AboutInfo.Blocks,
    /// sesuai urutannya.
    /// </summary>
    private void BuildBlocks()
    {
        BlocksPanel.Children.Clear();

        // Judul pertama tidak diberi jarak atas ekstra.
        bool isFirst = true;

        foreach (AboutBlock block in AboutInfo.Blocks)
        {
            UIElement? element = null;

            switch (block.Type)
            {
                case AboutBlockType.Heading:
                    element = CreateHeading(block.Text, isFirst);
                    break;

                case AboutBlockType.Paragraph:
                    element = CreateParagraph(block.Text);
                    break;

                case AboutBlockType.BulletList:
                    element = CreateBulletList(block.Items);
                    break;

                case AboutBlockType.BulletGroup:
                    element = CreateBulletGroup(block.Text, block.Items);
                    break;

                case AboutBlockType.Credit:
                    element = CreateCredit(block.Text, block.Items);
                    break;
            }

            if (element is null)
                continue;

            BlocksPanel.Children.Add(element);

            isFirst = false;
        }
    }

    // -------------------------------------------------------------
    // JUDUL
    // -------------------------------------------------------------

    /// <summary>
    /// Judul: tebal, rata tengah, dengan jarak ekstra di atasnya
    /// agar terpisah dari bagian sebelumnya.
    /// </summary>
    private static TextBlock CreateHeading(string text, bool isFirst)
    {
        var heading =
            new TextBlock
            {
                FontSize = HeadingFontSize,

                // Seluruh judul dibuat tebal.
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,

                TextAlignment = HeadingTextAlignment,

                TextWrapping = TextWrapping.Wrap,

                Margin = new Thickness(0, isFirst ? 0 : 8, 0, 0)
            };

        AddFormattedText(heading, text);

        return heading;
    }

    // -------------------------------------------------------------
    // PARAGRAF
    // -------------------------------------------------------------

    /// <summary>
    /// Paragraf biasa. Kata yang diapit ** ** ditampilkan tebal.
    /// </summary>
    private static TextBlock CreateParagraph(string text)
    {
        var paragraph =
            new TextBlock
            {
                FontSize = BodyFontSize,

                LineHeight = BodyLineHeight,

                TextAlignment = ParagraphTextAlignment,

                TextWrapping = TextWrapping.Wrap,

                // Teks bisa diblok dan disalin oleh pengguna.
                IsTextSelectionEnabled = true
            };

        AddFormattedText(paragraph, text);

        return paragraph;
    }

    // -------------------------------------------------------------
    // DAFTAR BUTIR
    // -------------------------------------------------------------

    /// <summary>
    /// Daftar butir satu tingkat (AboutBlock.Bullets).
    /// </summary>
    private static StackPanel CreateBulletList(string[] items)
    {
        var list =
            new StackPanel
            {
                Spacing = 6
            };

        foreach (string item in items)
        {
            if (string.IsNullOrWhiteSpace(item))
                continue;

            list.Children.Add(
                CreateBulletRow(
                    BulletMarker,
                    item,
                    isBold: false));
        }

        return list;
    }

    /// <summary>
    /// Butir bertingkat (AboutBlock.BulletGroup):
    ///
    ///   •  Judul butir (tebal)
    ///        ◦  isi 1
    ///        ◦  isi 2
    ///
    /// Butir induk tebal; butir anak menjorok sebesar SubBulletIndent.
    /// </summary>
    private static StackPanel CreateBulletGroup(
        string title,
        string[] items)
    {
        var group =
            new StackPanel
            {
                Spacing = 6
            };

        // ---------- Butir induk (tebal) ----------

        group.Children.Add(
            CreateBulletRow(
                BulletMarker,
                title,
                isBold: true));

        // ---------- Butir anak (menjorok) ----------

        var children =
            new StackPanel
            {
                Spacing = 6,

                // Jorokan ke kanan.
                Margin = new Thickness(SubBulletIndent, 0, 0, 0)
            };

        foreach (string item in items)
        {
            if (string.IsNullOrWhiteSpace(item))
                continue;

            children.Children.Add(
                CreateBulletRow(
                    SubBulletMarker,
                    item,
                    isBold: false));
        }

        group.Children.Add(children);

        return group;
    }

    /// <summary>
    /// Satu baris butir, terdiri dari dua kolom:
    /// kolom kiri berisi tanda butir, kolom kanan berisi teks.
    /// Dengan dua kolom, baris kedua dan seterusnya dari satu butir
    /// sejajar dengan teks (bukan menjorok ke bawah tanda butir).
    /// </summary>
    private static Grid CreateBulletRow(
        string marker,
        string text,
        bool isBold)
    {
        var row =
            new Grid
            {
                ColumnSpacing = 8
            };

        // Kolom 0: tanda butir (lebar tetap). Kolom 1: teks (sisa lebar).
        row.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(14)
            });

        row.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star)
            });

        var markerText =
            new TextBlock
            {
                Text = marker,

                FontSize = BodyFontSize,

                LineHeight = BodyLineHeight,

                HorizontalAlignment = HorizontalAlignment.Center
            };

        Grid.SetColumn(markerText, 0);

        var content =
            new TextBlock
            {
                FontSize = BodyFontSize,

                LineHeight = BodyLineHeight,

                TextAlignment = BulletTextAlignment,

                TextWrapping = TextWrapping.Wrap,

                IsTextSelectionEnabled = true
            };

        // Butir induk: seluruh teks tebal.
        if (isBold)
        {
            content.FontWeight =
                Microsoft.UI.Text.FontWeights.Bold;
        }

        AddFormattedText(content, text);

        Grid.SetColumn(content, 1);

        row.Children.Add(markerText);
        row.Children.Add(content);

        return row;
    }

    // -------------------------------------------------------------
    // SUSUNAN TIM
    // -------------------------------------------------------------

    /// <summary>
    /// Susunan tim (AboutBlock.Credit): jabatan tebal, lalu nama-nama
    /// satu per baris. Rata tengah (lihat CreditTextAlignment).
    ///
    ///        Ketua Tim
    ///   Agus Nurul Saripudin, S.Kom.
    /// </summary>
    private static StackPanel CreateCredit(string role, string[] names)
    {
        var credit =
            new StackPanel
            {
                Spacing = 2
            };

        // ---------- Jabatan (tebal) ----------

        credit.Children.Add(
            new TextBlock
            {
                Text = role,

                FontSize = BodyFontSize,

                FontWeight = Microsoft.UI.Text.FontWeights.Bold,

                TextAlignment = CreditTextAlignment,

                TextWrapping = TextWrapping.Wrap
            });

        // ---------- Nama-nama ----------

        foreach (string name in names)
        {
            if (string.IsNullOrWhiteSpace(name))
                continue;

            credit.Children.Add(
                new TextBlock
                {
                    Text = name,

                    FontSize = BodyFontSize,

                    TextAlignment = CreditTextAlignment,

                    TextWrapping = TextWrapping.Wrap,

                    IsTextSelectionEnabled = true
                });
        }

        return credit;
    }

    // -------------------------------------------------------------
    // PENEBALAN KATA
    // -------------------------------------------------------------

    /// <summary>
    /// Mengisi TextBlock dengan teks yang mendukung penebalan ringan:
    /// bagian yang diapit dua tanda bintang (**) ditampilkan TEBAL.
    ///
    /// Contoh: "**Qur'an Kemenag** adalah aplikasi resmi"
    ///   -> "Qur'an Kemenag" tebal, "adalah aplikasi resmi" biasa.
    ///
    /// Cara kerja: teks dipecah pada setiap "**". Potongan urutan
    /// ganjil (indeks 1, 3, 5, ...) berada DI DALAM tanda bintang,
    /// sehingga dibuat tebal. Potongan genap tidak.
    /// </summary>
    private static void AddFormattedText(TextBlock target, string text)
    {
        target.Inlines.Clear();

        string[] parts =
            text.Split(
                new[] { "**" },
                StringSplitOptions.None);

        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Length == 0)
                continue;

            var run =
                new Run
                {
                    Text = parts[i]
                };

            if (i % 2 == 1)
            {
                run.FontWeight =
                    Microsoft.UI.Text.FontWeights.Bold;
            }

            target.Inlines.Add(run);
        }
    }

    // -------------------------------------------------------------
    // VERSI APLIKASI
    // -------------------------------------------------------------

    /// <summary>
    /// Membaca nomor versi aplikasi dari proyek (Assembly).
    /// Untuk mengubahnya, atur Version/AssemblyVersion di
    /// QuranKemenag.csproj. Mengembalikan "-" jika tidak terbaca.
    /// </summary>
    private static string GetAppVersion()
    {
        try
        {
            Version? version =
                Assembly
                    .GetExecutingAssembly()
                    .GetName()
                    .Version;

            return version?.ToString(3) ?? "-";
        }
        catch
        {
            return "-";
        }
    }
}