using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

//using Windows.ApplicationModel.DataTransfer;

using QuranKemenag.Models;
using QuranKemenag.Models.Copy;
using QuranKemenag.Services.Copy;
using QuranKemenag.ViewModels;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace QuranKemenag.Views.Copy;

public sealed partial class CopyAyatDialog : UserControl
{
    // ================================================================
    // FIELD
    // ================================================================

    private MainViewModel? _mainViewModel;

    private QuranCopyService? _copyService;
 

    private bool _isLoaded;

    private bool _isNormalizingRange;


    // ================================================================
    // HASIL COPY
    // ================================================================

    public int FromSurahNumber
    {
        get;
        private set;
    }

    public int FromAyahNumber
    {
        get;
        private set;
    }

    public int ToSurahNumber
    {
        get;
        private set;
    }

    public int ToAyahNumber
    {
        get;
        private set;
    }


    /// <summary>
    /// Pilihan copy yang dibuat user.
    /// </summary>
    public CopyOptions? CopyOptionsResult
    {
        get;
        private set;
    }


    /// <summary>
    /// True jika user menekan tombol Salin
    /// dan proses copy berhasil.
    /// </summary>
    public bool IsCopyConfirmed
    {
        get;
        private set;
    }


    /// <summary>
    /// Hasil teks yang sudah diformat.
    /// </summary>
    public string? CopyResult
    {
        get;
        private set;
    }


    // ================================================================
    // CONSTRUCTOR
    // ================================================================

    public CopyAyatDialog()
    {
        InitializeComponent();

        // ============================================================
        // EVENT RADIO MODE COPY
        // ============================================================

        CurrentAyatRadio.Checked +=
            CurrentAyatRadio_Checked;

        RangeAyatRadio.Checked +=
            RangeAyatRadio_Checked;


        // ============================================================
        // EVENT SURAH
        // ============================================================

        FromSurahComboBox.SelectionChanged +=
            FromSurahComboBox_SelectionChanged;

        ToSurahComboBox.SelectionChanged +=
            ToSurahComboBox_SelectionChanged;


        // ============================================================
        // EVENT AYAT
        // ============================================================

        FromAyahComboBox.SelectionChanged +=
            FromAyahComboBox_SelectionChanged;

        ToAyahComboBox.SelectionChanged +=
            ToAyahComboBox_SelectionChanged;


        // ============================================================
        // EVENT CHECKBOX
        // ============================================================

        ArabicCheckBox.Checked +=
            ArabicCheckBox_Changed;

        ArabicCheckBox.Unchecked +=
            ArabicCheckBox_Changed;


        TranslationCheckBox.Checked +=
            ContentOptionChanged;

        TranslationCheckBox.Unchecked +=
            ContentOptionChanged;


        SignLanguageCheckBox.Checked +=
            ContentOptionChanged;

        SignLanguageCheckBox.Unchecked +=
            ContentOptionChanged;


        // ============================================================
        // LOADED
        // ============================================================

        Loaded +=
            CopyAyatDialog_Loaded;
    }


    // ================================================================
    // LOADED
    // ================================================================

    private void CopyAyatDialog_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        if (_isLoaded)
            return;

        _isLoaded = true;


        // ============================================================
        // APP SERVICES
        // ============================================================

        if (App.Services is null)
        {
            Debug.WriteLine(
                "[COPY] App.Services belum tersedia.");

            return;
        }


        // ============================================================
        // MAIN VIEW MODEL
        // ============================================================

        _mainViewModel =
            App.Services.GetService<MainViewModel>();


        if (_mainViewModel is null)
        {
            Debug.WriteLine(
                "[COPY] MainViewModel tidak ditemukan.");

            return;
        }


        // ============================================================
        // COPY SERVICE
        // ============================================================

        _copyService =
            App.Services.GetService<QuranCopyService>();


        if (_copyService is null)
        {
            Debug.WriteLine(
                "[COPY] QuranCopyService tidak ditemukan.");

            return;
        }


        // ============================================================
        // DAFTAR SURAH
        // ============================================================

        FromSurahComboBox.ItemsSource =
            _mainViewModel.Surahs;

        ToSurahComboBox.ItemsSource =
            _mainViewModel.Surahs;


        // ============================================================
        // TENTUKAN SURAH DEFAULT
        // ============================================================

        var selectedSurah =
            _mainViewModel.SelectedSurah;


        // Jika dialog dibuka dari Home,
        // SelectedSurah mungkin null.
        //
        // Gunakan Al-Fatihah sebagai default.

        if (selectedSurah is null)
        {
            selectedSurah =
                _mainViewModel.Surahs
                    .FirstOrDefault(
                        s => s.Id == 1);
        }


        if (selectedSurah is null)
        {
            Debug.WriteLine(
                "[COPY] Tidak ada data Surah.");

            return;
        }


        // ============================================================
        // SET FROM / TO SURAH
        // ============================================================

        FromSurahComboBox.SelectedItem =
            selectedSurah;

        ToSurahComboBox.SelectedItem =
            selectedSurah;


        // ============================================================
        // TENTUKAN AYAT DEFAULT
        // ============================================================

        int selectedAyah = 1;


        if (_mainViewModel.SelectedAyat is not null)
        {
            selectedAyah =
                _mainViewModel.SelectedAyat.AyatNumber;
        }


        // Pastikan ayat valid.

        if (selectedAyah < 1 ||
            selectedAyah > selectedSurah.TotalAyah)
        {
            selectedAyah = 1;
        }


        // ============================================================
        // ISI AYAT FROM / TO
        // ============================================================

        LoadAyahNumbers(
            FromAyahComboBox,
            selectedSurah.TotalAyah,
            selectedAyah);

        LoadAyahNumbers(
            ToAyahComboBox,
            selectedSurah.TotalAyah,
            selectedAyah);


        // ============================================================
        // DEFAULT MODE
        // ============================================================

        CurrentAyatRadio.IsChecked =
            true;


        // ============================================================
        // DEFAULT OPTIONS
        // ============================================================

        ArabicCheckBox.IsChecked =
            true;

        TranslationCheckBox.IsChecked =
            true;

        TransliterationCheckBox.IsChecked =
            false;

        SignLanguageCheckBox.IsChecked =
            false;

        TandaKurungCheckBox.IsChecked =
            false;

        SelangSelingCheckBox.IsChecked =
            false;

        NomorAyatCheckBox.IsChecked =
            false;

        SurahAyatCheckBox.IsChecked =
            false;

        ComputerFontRadioButton.IsChecked =
            true;


        // ============================================================
        // UPDATE UI
        // ============================================================

        ArabicCheckBox_Changed(
            this,
            new RoutedEventArgs());

        ContentOptionChanged(
            this,
            new RoutedEventArgs());


        // ============================================================
        // PASTIKAN RANGE PANEL SESUAI MODE
        // ============================================================

        if (RangeAyatRadio.IsChecked == true)
        {
            RangeAyatRadio_Checked(
                this,
                new RoutedEventArgs());
        }
        else
        {
            CurrentAyatRadio_Checked(
                this,
                new RoutedEventArgs());
        }


        // ============================================================
        // DEBUG
        // ============================================================

        Debug.WriteLine(
            "========================================");

        Debug.WriteLine(
            "[COPY DIALOG] Berhasil diinisialisasi.");

        Debug.WriteLine(
            $"[COPY DIALOG] From = " +
            $"{selectedSurah.LatinName}:{selectedAyah}");

        Debug.WriteLine(
            $"[COPY DIALOG] To = " +
            $"{selectedSurah.LatinName}:{selectedAyah}");

        Debug.WriteLine(
            $"[COPY DIALOG] Mode = " +
            $"{(RangeAyatRadio.IsChecked == true
                ? "Rentang Ayat"
                : "Ayat Saat Ini")}");

        Debug.WriteLine(
            "========================================");
    }


    // ================================================================
    // LOAD NOMOR AYAT
    // ================================================================

    private static void LoadAyahNumbers(
        ComboBox comboBox,
        int totalAyah,
        int selectedAyah = 1)
    {
        if (totalAyah <= 0)
        {
            comboBox.ItemsSource = null;
            return;
        }


        comboBox.ItemsSource =
            Enumerable
                .Range(
                    1,
                    totalAyah)
                .ToList();


        if (selectedAyah < 1 ||
            selectedAyah > totalAyah)
        {
            selectedAyah = 1;
        }


        comboBox.SelectedItem =
            selectedAyah;
    }


    // ================================================================
    // MODE AYAT SAAT INI
    // ================================================================

    private void CurrentAyatRadio_Checked(
        object sender,
        RoutedEventArgs e)
    {
        if (RangePanel != null)
        {
            RangePanel.Visibility =
                Visibility.Collapsed;
        }


        Debug.WriteLine(
            "[COPY UI] Mode = Ayat saat ini");
    }


    // ================================================================
    // MODE RENTANG AYAT
    // ================================================================

    private void RangeAyatRadio_Checked(
        object sender,
        RoutedEventArgs e)
    {
        if (RangePanel != null)
        {
            RangePanel.Visibility =
                Visibility.Visible;
        }


        Debug.WriteLine(
            "[COPY UI] Mode = Rentang ayat");


        // Pastikan data From / To tersedia.

        if (FromSurahComboBox.SelectedItem is Surah fromSurah)
        {
            if (FromAyahComboBox.ItemsSource is null)
            {
                LoadAyahNumbers(
                    FromAyahComboBox,
                    fromSurah.TotalAyah);
            }
        }


        if (ToSurahComboBox.SelectedItem is Surah toSurah)
        {
            if (ToAyahComboBox.ItemsSource is null)
            {
                LoadAyahNumbers(
                    ToAyahComboBox,
                    toSurah.TotalAyah);
            }
        }


        UpdateToRange();
    }


    // ================================================================
    // FROM SURAH BERUBAH
    // ================================================================

    private void FromSurahComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (_isNormalizingRange)
            return;


        if (FromSurahComboBox.SelectedItem
            is not Surah surah)
        {
            FromAyahComboBox.ItemsSource =
                null;

            return;
        }


        // Muat nomor ayat berdasarkan
        // jumlah ayat Surah From.

        LoadAyahNumbers(
            FromAyahComboBox,
            surah.TotalAyah);


        Debug.WriteLine(
            $"[COPY UI] From Surah = " +
            $"{surah.LatinName}");


        UpdateToRange();
    }


    // ================================================================
    // TO SURAH BERUBAH
    // ================================================================

    private void ToSurahComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (_isNormalizingRange)
            return;


        if (ToSurahComboBox.SelectedItem
            is not Surah surah)
        {
            ToAyahComboBox.ItemsSource =
                null;

            return;
        }


        // Muat nomor ayat berdasarkan
        // jumlah ayat Surah To.

        LoadAyahNumbers(
            ToAyahComboBox,
            surah.TotalAyah);


        Debug.WriteLine(
            $"[COPY UI] To Surah = " +
            $"{surah.LatinName}");


        UpdateToRange();
    }


    // ================================================================
    // FROM AYAT BERUBAH
    // ================================================================

    private void FromAyahComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (_isNormalizingRange)
            return;


        if (FromAyahComboBox.SelectedItem
            is not int fromAyah)
        {
            return;
        }


        Debug.WriteLine(
            $"[COPY UI] From Ayat = {fromAyah}");


        UpdateToRange();
    }


    // ================================================================
    // TO AYAT BERUBAH
    // ================================================================

    private void ToAyahComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (_isNormalizingRange)
            return;


        if (ToAyahComboBox.SelectedItem
            is not int toAyah)
        {
            return;
        }


        Debug.WriteLine(
            $"[COPY UI] To Ayat = {toAyah}");


        UpdateToRange();
    }


    // ================================================================
    // VALIDASI RANGE
    //
    // FROM adalah titik awal.
    //
    // TO tidak boleh:
    //
    // 1. Berada pada surah sebelum FROM.
    //
    // 2. Jika surah sama,
    //    ayat TO tidak boleh lebih kecil
    //    dari ayat FROM.
    //
    // FROM TIDAK PERNAH DIUBAH.
    // ================================================================

    private void UpdateToRange()
    {
        if (_isNormalizingRange)
            return;


        // ============================================================
        // AMBIL FROM SURAH
        // ============================================================

        if (FromSurahComboBox.SelectedItem
            is not Surah fromSurah)
        {
            return;
        }


        // ============================================================
        // AMBIL TO SURAH
        // ============================================================

        if (ToSurahComboBox.SelectedItem
            is not Surah toSurah)
        {
            return;
        }


        // ============================================================
        // AMBIL FROM AYAT
        // ============================================================

        if (FromAyahComboBox.SelectedItem
            is not int fromAyah)
        {
            return;
        }


        // ============================================================
        // AMBIL TO AYAT
        // ============================================================

        if (ToAyahComboBox.SelectedItem
            is not int toAyah)
        {
            return;
        }


        // ============================================================
        // TO SURAH LEBIH KECIL DARI FROM
        // ============================================================

        if (toSurah.Id < fromSurah.Id)
        {
            Debug.WriteLine(
                "[COPY UI] To Surah lebih kecil dari From.");

            Debug.WriteLine(
                $"[COPY UI] From = " +
                $"{fromSurah.Id}:{fromAyah}");

            Debug.WriteLine(
                $"[COPY UI] To lama = " +
                $"{toSurah.Id}:{toAyah}");


            _isNormalizingRange = true;

            try
            {
                // FROM TIDAK DIUBAH.
                //
                // TO diarahkan ke FROM.

                ToSurahComboBox.SelectedItem =
                    fromSurah;


                // Muat daftar ayat
                // berdasarkan From Surah.

                LoadAyahNumbers(
                    ToAyahComboBox,
                    fromSurah.TotalAyah);


                // To minimal sama dengan From.

                ToAyahComboBox.SelectedItem =
                    fromAyah;
            }
            finally
            {
                _isNormalizingRange = false;
            }


            Debug.WriteLine(
                $"[COPY UI] To diarahkan menjadi " +
                $"{fromSurah.LatinName}:{fromAyah}");

            return;
        }


        // ============================================================
        // TO SURAH LEBIH BESAR DARI FROM
        // ============================================================

        if (toSurah.Id > fromSurah.Id)
        {
            // Valid.
            //
            // Contoh:
            //
            // From = Al-Baqarah : 6
            // To   = Ali Imran  : 3
            //
            // Tidak ada perubahan.

            return;
        }


        // ============================================================
        // SURAH SAMA
        // ============================================================

        if (toSurah.Id == fromSurah.Id)
        {
            // ========================================================
            // TO AYAT LEBIH KECIL DARI FROM
            // ========================================================

            if (toAyah < fromAyah)
            {
                Debug.WriteLine(
                    $"[COPY UI] To Ayat {toAyah} " +
                    $"lebih kecil dari From Ayat {fromAyah}.");


                _isNormalizingRange = true;

                try
                {
                    // FROM TETAP.
                    //
                    // TO diarahkan ke From.

                    ToAyahComboBox.SelectedItem =
                        fromAyah;
                }
                finally
                {
                    _isNormalizingRange = false;
                }


                Debug.WriteLine(
                    $"[COPY UI] To Ayat diarahkan " +
                    $"menjadi {fromAyah}");
            }
        }
    }


    // ================================================================
    // ARABIC CHECKBOX
    // ================================================================

    private void ArabicCheckBox_Changed(
        object sender,
        RoutedEventArgs e)
    {
        bool arabicSelected =
            ArabicCheckBox.IsChecked == true;


        ComputerFontRadioButton.IsEnabled =
            arabicSelected;


        HandwrittenFontRadioButton.IsEnabled =
            arabicSelected;


        if (!arabicSelected)
        {
            ComputerFontRadioButton.IsChecked =
                true;
        }
    }


    // ================================================================
    // CONTENT OPTION
    // ================================================================

    private void ContentOptionChanged(
        object sender,
        RoutedEventArgs e)
    {
        bool hasSecondaryContent =
            TranslationCheckBox.IsChecked == true ||
            SignLanguageCheckBox.IsChecked == true;


        SelangSelingCheckBox.IsEnabled =
            hasSecondaryContent;


        if (!hasSecondaryContent)
        {
            SelangSelingCheckBox.IsChecked =
                false;
        }
    }

    // ================================================================
    // FORMAT COPY
    // ================================================================

    private string GetSelectedCopyFormat()
    {
        if (FormatComboBox.SelectedItem
            is ComboBoxItem item)
        {
            return item.Tag?.ToString()
                   ?? "Word";
        }

        return "Word";
    }


    // ================================================================
    // COPY
    // ================================================================

    // ================================================================
    // COPY BUTTON
    // ================================================================

    private async void CopyButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            // ========================================================
            // 1. VALIDASI SERVICE
            // ========================================================

            if (_copyService is null)
            {
                Debug.WriteLine(
                    "[COPY] ERROR: QuranCopyService belum tersedia.");

                await ShowMessageAsync(
                    "Service copy belum tersedia.");

                return;
            }

            if (_mainViewModel is null)
            {
                Debug.WriteLine(
                    "[COPY] ERROR: MainViewModel belum tersedia.");

                await ShowMessageAsync(
                    "MainViewModel belum tersedia.");

                return;
            }


            // ========================================================
            // 2. AMBIL FROM SURAH
            // ========================================================

            if (FromSurahComboBox.SelectedItem
                is not Surah fromSurah)
            {
                Debug.WriteLine(
                    "[COPY] ERROR: From Surah belum dipilih.");

                await ShowMessageAsync(
                    "Silakan pilih Surah awal.");

                return;
            }


            // ========================================================
            // 3. AMBIL FROM AYAT
            // ========================================================

            if (FromAyahComboBox.SelectedItem
                is not int fromAyah)
            {
                Debug.WriteLine(
                    "[COPY] ERROR: From Ayat belum dipilih.");

                await ShowMessageAsync(
                    "Silakan pilih ayat awal.");

                return;
            }


            // ========================================================
            // 4. TENTUKAN TO
            // ========================================================

            int toSurahNumber;
            int toAyahNumber;


            // ========================================================
            // AYAT SAAT INI
            // ========================================================

            if (CurrentAyatRadio.IsChecked == true)
            {
                toSurahNumber =
                    fromSurah.Id;

                toAyahNumber =
                    fromAyah;
            }


            // ========================================================
            // RANGE AYAT
            // ========================================================

            else
            {
                if (ToSurahComboBox.SelectedItem
                    is not Surah toSurah)
                {
                    Debug.WriteLine(
                        "[COPY] ERROR: To Surah belum dipilih.");

                    await ShowMessageAsync(
                        "Silakan pilih Surah akhir.");

                    return;
                }


                if (ToAyahComboBox.SelectedItem
                    is not int toAyah)
                {
                    Debug.WriteLine(
                        "[COPY] ERROR: To Ayat belum dipilih.");

                    await ShowMessageAsync(
                        "Silakan pilih ayat akhir.");

                    return;
                }


                toSurahNumber =
                    toSurah.Id;

                toAyahNumber =
                    toAyah;
            }


            // ========================================================
            // 5. VALIDASI URUTAN RANGE
            //
            // FROM tidak boleh lebih besar daripada TO.
            //
            // Contoh valid:
            //
            // From : Al-Fatihah 3
            // To   : Al-Baqarah 1
            //
            // Contoh tidak valid:
            //
            // From : Al-Baqarah 5
            // To   : Al-Fatihah 3
            //
            // UI seharusnya sudah mencegah kondisi ini,
            // tetapi tetap kita validasi di sini.
            // ========================================================

            bool invalidRange =
                fromSurah.Id > toSurahNumber ||
                (
                    fromSurah.Id == toSurahNumber &&
                    fromAyah > toAyahNumber
                );


            if (invalidRange)
            {
                Debug.WriteLine(
                    "[COPY] ERROR: Range tidak valid.");

                Debug.WriteLine(
                    $"[COPY] From = " +
                    $"{fromSurah.Id}:{fromAyah}");

                Debug.WriteLine(
                    $"[COPY] To = " +
                    $"{toSurahNumber}:{toAyahNumber}");

                await ShowMessageAsync(
                    "Range ayat tidak valid.\n\n" +
                    "Ayat akhir tidak boleh berada sebelum ayat awal.");

                return;
            }


            // ========================================================
            // 6. AMBIL PILIHAN COPY
            // ========================================================

            var options =
                new CopyOptions
                {
                    IncludeArabic =
                        ArabicCheckBox.IsChecked == true,

                    IncludeTranslation =
                        TranslationCheckBox.IsChecked == true,

                    IncludeTransliteration =
                        TransliterationCheckBox.IsChecked == true,

                    IncludeSignLanguage =
                        SignLanguageCheckBox.IsChecked == true,

                    AddParentheses =
                        TandaKurungCheckBox.IsChecked == true,

                    AlternateContent =
                        SelangSelingCheckBox.IsChecked == true,

                    AddAyahNumber =
                        NomorAyatCheckBox.IsChecked == true,

                    AddSurahAyah =
                        SurahAyatCheckBox.IsChecked == true,

                    ArabicFont =
                        HandwrittenFontRadioButton.IsChecked == true
                            ? ArabicCopyFont.Handwritten
                            : ArabicCopyFont.ComputerFont
                };


            // ========================================================
            // 7. DEBUG INFORMASI COPY
            // ========================================================

            Debug.WriteLine(
                "========================================");

            Debug.WriteLine(
                "[COPY] MULAI PROSES COPY");

            Debug.WriteLine(
                $"[COPY] From = " +
                $"Surah {fromSurah.Id}, Ayat {fromAyah}");

            Debug.WriteLine(
                $"[COPY] To   = " +
                $"Surah {toSurahNumber}, Ayat {toAyahNumber}");

            Debug.WriteLine(
                $"[COPY] Font = " +
                $"{options.ArabicFont}");

            Debug.WriteLine(
                $"[COPY] Arabic = " +
                $"{options.IncludeArabic}");

            Debug.WriteLine(
                $"[COPY] Translation = " +
                $"{options.IncludeTranslation}");

            Debug.WriteLine(
                $"[COPY] Transliteration = " +
                $"{options.IncludeTransliteration}");

            Debug.WriteLine(
                $"[COPY] SignLanguage = " +
                $"{options.IncludeSignLanguage}");

            Debug.WriteLine(
                $"[COPY] Parentheses = " +
                $"{options.AddParentheses}");

            Debug.WriteLine(
                $"[COPY] Alternate = " +
                $"{options.AlternateContent}");

            Debug.WriteLine(
                $"[COPY] AyahNumber = " +
                $"{options.AddAyahNumber}");

            Debug.WriteLine(
                $"[COPY] SurahAyah = " +
                $"{options.AddSurahAyah}");


            // ========================================================
            // 8. AMBIL DATA AYAT
            // ========================================================

            var ayats =
                await _copyService.GetRangeAsync(
                    fromSurah.Id,
                    fromAyah,
                    toSurahNumber,
                    toAyahNumber,
                    options.ArabicFont);


            // ========================================================
            // 9. VALIDASI DATA
            // ========================================================

            if (ayats is null ||
                ayats.Count == 0)
            {
                Debug.WriteLine(
                    "[COPY] ERROR: Tidak ada ayat yang ditemukan.");

                await ShowMessageAsync(
                    "Ayat yang dipilih tidak ditemukan.");

                return;
            }


            Debug.WriteLine(
                $"[COPY] Jumlah ayat = " +
                $"{ayats.Count}");


            // ========================================================
            // 10. NAMA SURAH
            // ========================================================

            var surahNames =
                _mainViewModel.Surahs
                    .ToDictionary(
                        s => s.Id,
                        s => s.LatinName);


            // ========================================================
            // 11. FORMATTER
            // ========================================================

            var formatter =
                new QuranCopyFormatter();


            string result =
                formatter.Format(
                    ayats,
                    options,
                    surahNames);

            var copyResult =
                formatter.FormatBlocks(
                    ayats,
                    options,
                    surahNames);


            // ========================================================
            // 12. VALIDASI HASIL FORMAT
            // ========================================================

            if (string.IsNullOrWhiteSpace(result))
            {
                Debug.WriteLine(
                    "[COPY] ERROR: Hasil formatter kosong.");

                await ShowMessageAsync(
                    "Hasil copy kosong.");

                return;
            }


            // ========================================================
            // 13. SIMPAN RANGE
            // ========================================================

            FromSurahNumber =
                fromSurah.Id;

            FromAyahNumber =
                fromAyah;

            ToSurahNumber =
                toSurahNumber;

            ToAyahNumber =
                toAyahNumber;


            // ========================================================
            // 14. SIMPAN HASIL COPY
            // ========================================================

            CopyResult =
                result;

            CopyOptionsResult =
                options;


            // ========================================================
            // 15. AMBIL FORMAT TUJUAN
            // ========================================================

            string copyFormat =
                GetSelectedCopyFormat();


            Debug.WriteLine(
                $"[COPY] Format = {copyFormat}");


            // ========================================================
            // 16. PROSES BERDASARKAN FORMAT
            // ========================================================

            switch (copyFormat)
            {
                // ====================================================
                // MICROSOFT WORD
                // ====================================================

                case "Word":
                    {
                        Debug.WriteLine(
                            "[COPY] Mengirim hasil ke Microsoft Word.");


                        var wordCopyService =
                            new WordCopyService();


                        wordCopyService.CopyToWord(copyResult);

                        Debug.WriteLine(
                            "[COPY] Proses Word selesai.");

                        break;
                    }


                // ====================================================
                // MICROSOFT POWERPOINT
                // ====================================================

                case "PowerPoint":
                    {
                        Debug.WriteLine(
                            "[COPY] Mengirim hasil ke Microsoft PowerPoint.");


                        var powerPointCopyService =
                            new PowerPointCopyService();


                        powerPointCopyService.CopyToPowerPoint(
                            result);


                        Debug.WriteLine(
                            "[COPY] Proses PowerPoint selesai.");

                        break;
                    }


                // ====================================================
                // FORMAT TIDAK DIKENALI
                // ====================================================

                default:
                    {
                        Debug.WriteLine(
                            $"[COPY] Format tidak dikenali: " +
                            $"{copyFormat}");

                        await ShowMessageAsync(
                            "Format copy tidak dikenali.");

                        return;
                    }
            }


            // ========================================================
            // 17. DEBUG HASIL COPY
            // ========================================================

            Debug.WriteLine(
                "========== HASIL COPY ==========");

            Debug.WriteLine(
                result);

            Debug.WriteLine(
                "========== AKHIR COPY ==========");


            // ========================================================
            // 18. COPY BERHASIL
            // ========================================================

            IsCopyConfirmed =
                true;


            // ========================================================
            // 19. TUTUP DIALOG
            // ========================================================

            CloseDialog();
        }
        catch (Exception ex)
        {
            // ========================================================
            // ERROR HANDLER
            // ========================================================

            Debug.WriteLine(
                "========== COPY ERROR ==========");

            Debug.WriteLine(
                ex.ToString());

            Debug.WriteLine(
                "================================");


            try
            {
                await ShowMessageAsync(
                    "Terjadi kesalahan saat melakukan copy.\n\n" +
                    ex.Message);
            }
            catch
            {
                // Jangan biarkan error dialog
                // menutupi error utama.
            }
        }
    }


    // ================================================================
    // CLOSE BUTTON / X
    // ================================================================

    private void CloseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Debug.WriteLine(
            "[COPY] Tombol X diklik.");

        IsCopyConfirmed =
            false;

        CloseDialog();
    }


    // ================================================================
    // CANCEL BUTTON
    // ================================================================

    private void CancelButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Debug.WriteLine(
            "[COPY] Tombol Batal diklik.");

        IsCopyConfirmed =
            false;

        CloseDialog();
    }


    // ================================================================
    // MESSAGE
    // ================================================================

    private async Task ShowMessageAsync(
        string message)
    {
        if (XamlRoot is null)
        {
            Debug.WriteLine(
                $"[COPY MESSAGE] {message}");

            return;
        }


        var dialog =
            new ContentDialog
            {
                Title = "Copy Ayat",

                Content =
                    message,

                CloseButtonText =
                    "OK",

                XamlRoot =
                    XamlRoot
            };


        await dialog.ShowAsync();
    }


    // ================================================================
    // CLOSE DIALOG
    // ================================================================

    private void CloseDialog()
    {
        DependencyObject? current =
            this;


        while (current != null)
        {
            if (current is ContentDialog dialog)
            {
                dialog.Hide();

                return;
            }


            current =
                VisualTreeHelper.GetParent(
                    current);
        }


        Debug.WriteLine(
            "[COPY] ContentDialog parent tidak ditemukan.");
    }
}