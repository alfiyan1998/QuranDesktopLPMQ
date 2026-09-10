using QuranKemenag.Services.Page;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QuranKemenag.Models;
using QuranKemenag.Repositories;
using QuranKemenag.Services;
using QuranKemenag.ViewModels;
using QuranKemenag.Views;
using QuranKemenag.Views.Copy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace QuranKemenag;

/// <summary>
/// Code-behind MainWindow: hanya bertugas mengikat ViewModel dan memicu pemuatan data.
/// </summary>
public sealed partial class MainWindow : Window
{
    private bool _isInitialized;
    private bool _isSidebarVisible = true;

    public MainWindow()
    {
        this.InitializeComponent();

        //TestMushafLayout();

        Title = "Quran Kemenag for Windows";

        // Do not use DI or ViewModel for this repository test.
        Activated += MainWindow_Activated;
    }

    private static IReadOnlyList<JuzInfo> CreateJuzList()
    {
        return Enumerable
            .Range(1, 30)
            .Select(number => new JuzInfo
            {
                JuzNumber = number
            })
            .ToList();
    }

    /// <summary>
    /// Menampilkan salah satu view utama dan menyembunyikan view lainnya.
    /// </summary>
    private void ShowView(
        UserControl view)
    {
        HomeView.Visibility =
            view == HomeView
                ? Visibility.Visible
                : Visibility.Collapsed;

        SurahView.Visibility =
            view == SurahView
                ? Visibility.Visible
                : Visibility.Collapsed;

        JuzView.Visibility =
           view == JuzView
               ? Visibility.Visible
               : Visibility.Collapsed;

        ReaderView.Visibility =
            view == ReaderView
                ? Visibility.Visible
                : Visibility.Collapsed;
    }

    private async void MainWindow_Activated(object sender, WindowActivatedEventArgs e)
    {
        if (_isInitialized)
            return;

        _isInitialized = true;

        try
        {
            if (App.Services is null)
            {
                throw new InvalidOperationException("DI container (App.Services) belum tersedia.");
            }

            var vm = App.Services.GetRequiredService<MainViewModel>();




            // Set DataContext of the root element to the MainViewModel
            if (this.Content is FrameworkElement root && root.DataContext != vm)
            {
                root.DataContext = vm;

                // Wire sidebar navigation events so selecting 'Page' shows MushafPageView
                // and selecting Surah/Home returns to Ayat view.
                try
                {
                    Sidebar.HomeClicked -= Sidebar_HomeClicked;
                    Sidebar.SurahClicked -= Sidebar_SurahClicked;
                    Sidebar.PageClicked -= Sidebar_PageClicked;

                    Sidebar.HomeClicked += Sidebar_HomeClicked;
                    Sidebar.SurahClicked += Sidebar_SurahClicked;
                    Sidebar.PageClicked += Sidebar_PageClicked;

                    Sidebar.JuzClicked -= Sidebar_JuzClicked;
                    Sidebar.JuzClicked += Sidebar_JuzClicked;

                    // =====================================================
                    // TOOLBAR
                    // =====================================================

                    Toolbar.SidebarToggleRequested -=
                        Toolbar_SidebarToggleRequested;

                    Toolbar.SidebarToggleRequested +=
                        Toolbar_SidebarToggleRequested;

                    // Event ketika user memilih salah satu surah.
                    SurahView.SurahSelected -= SurahView_SurahSelected;
                    SurahView.SurahSelected += SurahView_SurahSelected;

                    JuzView.JuzSelected -= JuzView_JuzSelected;
                    JuzView.JuzSelected += JuzView_JuzSelected;

                    // =====================================================
                    // TOOLBAR SURAH
                    // =====================================================

                    Toolbar.SurahSelectionRequested -=
                        Toolbar_SurahSelectionRequested;

                    Toolbar.SurahSelectionRequested +=
                        Toolbar_SurahSelectionRequested;

                    // =====================================================
                    // TOOLBAR COPY
                    // =====================================================

                    Toolbar.CopyRequested -=
                        Toolbar_CopyRequested;

                    Toolbar.CopyRequested +=
                        Toolbar_CopyRequested;
                }
                catch { /* ignore if Sidebar not available yet */ }
            }

            // Load surah once
            await vm.LoadSurahAsync();

            // Verify count
            Debug.WriteLine($"Jumlah Surah : {vm.Surahs.Count}");

            // ----------------------------
            // Temporary test: gunakan MainViewModel.LoadAyatAsync untuk memuat ayat surah 1
            // Ini menunggu method pada ViewModel yang memanggil repository dan kemudian
            // mengisi ObservableCollection<Ayat> Ayats. Hanya untuk pengujian sementara.
            // ----------------------------
            //try
            //{
            // Gunakan ViewModel yang sudah di-resolve di atas (vm)
            //await vm.LoadAyatAsync(1);

            // Tampilkan jumlah dan isi koleksi Ayats yang telah diisi oleh ViewModel
            //    Debug.WriteLine($"Jumlah Ayat : {vm.Ayats.Count}");

            //    foreach (var a in vm.Ayats)
            //    {
            //        Debug.WriteLine($"{a.AyahNumber} - {a.ArabicText}");
            //    }
            //}
            //catch (Exception rex)
            //{
            //    Debug.WriteLine("Error while testing LoadAyatAsync: " + rex);
            //}

            // No experimental windows. Application will continue to show the normal Reader view.
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
               "ERROR MAIN WINDOW:");
            Debug.WriteLine(ex.ToString());
        }
    }

    /// <summary>Menampilkan/menyembunyikan Sidebar saat tombol hamburger ditekan.</summary>
    /// <summary>
    /// Menampilkan / menyembunyikan Sidebar
    /// ketika tombol hamburger pada Toolbar ditekan.
    /// </summary>
    private void Toolbar_SidebarToggleRequested(
    object sender,
    RoutedEventArgs e)
    {
        _isSidebarVisible = !_isSidebarVisible;

        if (_isSidebarVisible)
        {
            Sidebar.Visibility =
                Visibility.Visible;

            SidebarColumn.Width =
                new GridLength(220);
        }
        else
        {
            Sidebar.Visibility =
                Visibility.Collapsed;

            SidebarColumn.Width =
                new GridLength(0);
        }
    }

    private void Sidebar_HomeClicked(
    object? sender,
    EventArgs e)
    {
        if (App.Services is null)
            return;

        var vm =
            App.Services.GetService<MainViewModel>();

        if (vm is null)
            return;

        Debug.WriteLine(
            "[HOME] Kembali ke Home");

        // =====================================================
        // RESET READER CONTEXT
        // =====================================================

        vm.SelectedSurah = null;
        vm.SelectedAyat = null;

        // Pastikan tampilan kembali ke mode Ayat.
        vm.DisplayViewMode =
            QuranDisplayMode.Ayat;

        // =====================================================
        // TAMPILKAN HOME
        // =====================================================

        ShowView(HomeView);
    }

    private void Sidebar_SurahClicked(
    object? sender,
    EventArgs e)
    {
        if (App.Services is null)
            return;

        var vm =
            App.Services.GetService<MainViewModel>();

        if (vm is null)
            return;

        // =====================================================
        // RESET READER CONTEXT
        // =====================================================

        vm.SelectedSurah = null;
        vm.SelectedAyat = null;

        // Surah selalu dibuka sebagai daftar surah.
        vm.DisplayViewMode =
            QuranDisplayMode.Ayat;

        // Isi daftar 114 surah.
        SurahView.SetSurahs(vm.Surahs);

        // Tampilkan SurahView.
        ShowView(SurahView);
    }

    private async void SurahView_SurahSelected(
    object? sender,
    Surah surah)
    {
        if (App.Services is null)
            return;

        var vm =
            App.Services.GetService<MainViewModel>();

        if (vm is null)
            return;

        try
        {
            Debug.WriteLine(
                $"[SURAH NAVIGATION] " +
                $"Surah={surah.Id} " +
                $"Name={surah.LatinName}");

            // =====================================================
            // 1. PILIH SURAH
            // =====================================================

            vm.SelectedSurah = surah;


            // =====================================================
            // 2. SELALU MULAI DARI MODE AYAT
            // =====================================================

            vm.DisplayViewMode =
                QuranDisplayMode.Ayat;


            // =====================================================
            // 3. LOAD AYAT
            // =====================================================

            await vm.LoadAyatAsync(surah.Id);


            // =====================================================
            // 4. PILIH AYAT PERTAMA
            // =====================================================

            var firstAyat =
                vm.Ayats.FirstOrDefault();

            if (firstAyat is not null)
            {
                vm.SelectedAyat = firstAyat;
            }


            // =====================================================
            // 5. TAMPILKAN READER
            // =====================================================

            ShowView(ReaderView);


            Debug.WriteLine(
                $"[SURAH NAVIGATION] " +
                $"Reader dibuka: {surah.LatinName} " +
                $"Ayat=1 Mode=Ayat");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                "[SURAH NAVIGATION ERROR]");

            Debug.WriteLine(ex);
        }
    }

    private async void Sidebar_PageClicked(
    object? sender,
    EventArgs e)
    {
        if (App.Services is null)
            return;

        var vm =
            App.Services.GetService<MainViewModel>();

        if (vm is null)
            return;

        // =====================================================
        // RESET READER CONTEXT
        // =====================================================

        vm.SelectedSurah = null;
        vm.SelectedAyat = null;

        try
        {
            Debug.WriteLine(
                "[PAGE NAVIGATION] Navbar Page diklik");

            // =====================================================
            // Page selalu dibuka dari HALAMAN 1 (Al-Fatihah),
            // apa pun context sebelumnya (Surah lain / Juz).
            //
            // Toolbar (combo Surah + combo Ayat) harus mengikuti:
            // Surah = Al-Fatihah, Ayat = 1.
            // =====================================================

            var alFatihah =
                vm.Surahs.FirstOrDefault(
                    s => s.Id == 1);

            if (alFatihah is not null)
            {
                // Set SelectedSurah = Al-Fatihah.
                // Ini juga otomatis membersihkan context Juz
                // sebelumnya (lihat OnSelectedSurahChanged).
                vm.SelectedSurah = alFatihah;

                await vm.LoadAyatAsync(
                    alFatihah.Id);

                var firstAyat =
                    vm.Ayats.FirstOrDefault();

                if (firstAyat is not null)
                {
                    vm.SelectedAyat = firstAyat;
                }
            }

            // Page selalu menggunakan mode Mushaf.
            // Mengubah DisplayViewMode ke Halaman akan otomatis
            // membuka halaman berdasarkan SelectedAyat aktif
            // (Al-Fatihah ayat 1 → Halaman 1).
            vm.DisplayViewMode =
                QuranDisplayMode.Halaman;

            ShowView(ReaderView);

            Debug.WriteLine(
                $"[PAGE NAVIGATION] Dibuka: Surah=" +
                $"{vm.SelectedSurah?.LatinName} " +
                $"Ayat={vm.SelectedAyat?.AyatNumber} " +
                $"Halaman={vm.CurrentPageNumber}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                "[PAGE NAVIGATION ERROR]");

            Debug.WriteLine(ex);
        }
    }

    private void Sidebar_JuzClicked(
    object? sender,
    EventArgs e)
    {
        if (App.Services is null)
            return;

        var vm =
            App.Services.GetService<MainViewModel>();

        if (vm is null)
            return;

        // =====================================================
        // RESET READER CONTEXT
        // =====================================================

        vm.SelectedSurah = null;
        vm.SelectedAyat = null;

        Debug.WriteLine(
            "[JUZ NAVIGATION] Membuka daftar Juz");

        // Pastikan JuzView yang sudah ada di MainWindow
        // digunakan, bukan membuat JuzView baru.
        ShowView(JuzView);
    }

    private async void JuzView_JuzSelected(
    object? sender,
    JuzInfo juz)
    {
        if (App.Services is null)
            return;

        var vm =
            App.Services.GetService<MainViewModel>();

        if (vm is null)
            return;

        try
        {
            Debug.WriteLine(
                "========================================");

            Debug.WriteLine(
                "[JUZ NAVIGATION] Juz dipilih");

            Debug.WriteLine(
                $"Juz       : {juz.JuzNumber}");

            Debug.WriteLine(
                $"Surah     : {juz.SurahNumber}");

            Debug.WriteLine(
                $"Ayat awal : {juz.AyatNumber}");

            Debug.WriteLine(
                "========================================");


            // =====================================================
            // 1. MODE AWAL
            // =====================================================
            //
            // Ketika user memilih Juz,
            // kita mulai dari tampilan Ayat.
            //
            // Jangan memilih SelectedSurah di sini.
            // Jangan memanggil LoadAyatAsync().
            //
            vm.DisplayViewMode =
                QuranDisplayMode.Ayat;


            // =====================================================
            // 2. LOAD SELURUH JUZ
            // =====================================================
            //
            // LoadJuzAsync bertanggung jawab untuk:
            //
            // - mengambil seluruh ayat Juz
            // - menentukan ayat pertama Juz
            // - menentukan Surah pertama
            // - membangun Pages
            //
            await vm.LoadJuzAsync(
                juz.JuzNumber);


            // =====================================================
            // 3. TAMPILKAN READER
            // =====================================================

            ShowView(
                ReaderView);


            // =====================================================
            // 4. DEBUG
            // =====================================================

            Debug.WriteLine(
                "========================================");

            Debug.WriteLine(
                "[JUZ NAVIGATION] Reader berhasil dibuka");

            Debug.WriteLine(
                $"Juz        = {juz.JuzNumber}");

            Debug.WriteLine(
                $"Jumlah Ayat = {vm.Ayats.Count}");

            Debug.WriteLine(
                $"Surah      = " +
                $"{vm.SelectedSurah?.LatinName}");

            Debug.WriteLine(
                $"Ayat       = " +
                $"{vm.SelectedAyat?.AyatNumber}");

            Debug.WriteLine(
                $"Mode       = " +
                $"{vm.DisplayViewMode}");

            Debug.WriteLine(
                "========================================");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                "========================================");

            Debug.WriteLine(
                "[JUZ NAVIGATION ERROR]");

            Debug.WriteLine(
                ex.ToString());

            Debug.WriteLine(
                "========================================");
        }
    }

    /// <summary>Menampilkan/menyembunyikan Feature Panel saat tombolnya ditekan.</summary>
    private void Toolbar_PanelToggleRequested(object sender, RoutedEventArgs e)
    {
        // Implementasi UI state yang sudah ada tetap sama (tidak diubah).
    }

    private async Task TestJuzRepositoryAsync()
    {
        try
        {
            var repository =
                App.Services.GetRequiredService<IQuranRepository>();

            await JuzRepositoryDebugTest.RunAsync(repository);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                "[JUZ TEST ERROR]");

            Debug.WriteLine(
                ex.ToString());
        }
    }

    private void TestMushafLayout()
    {
        var databasePath = Path.Combine(
            AppContext.BaseDirectory,
            "Database",
            "mushaf_layout.db");

        Debug.WriteLine("--------------------------------");
        Debug.WriteLine("[MUSHAF] Test database layout");
        Debug.WriteLine("--------------------------------");

        if (!File.Exists(databasePath))
        {
            Debug.WriteLine(
                "[MUSHAF] DATABASE TIDAK DITEMUKAN!");

            return;
        }

        var repository =
            new LayoutRepository(databasePath);

        var lines =
            repository.GetPageLines(3);

        Debug.WriteLine(
            $"[MUSHAF] Jumlah baris = {lines.Count}");

        foreach (var line in lines)
        {
            Debug.WriteLine(
                $"LINE {line.LineNumber} | " +
                $"AYAT {line.VerseRange} | " +
                $"WORDS {line.Words.Count}");

            foreach (var word in line.Words)
            {
                Debug.WriteLine(
                    $"   {word.WordOrder}. " +
                    $"{word.Location} | " +
                    $"{word.WordText}");
            }
        }

        Debug.WriteLine("--------------------------------");
    }

    private async void Toolbar_SurahSelectionRequested(
    object sender,
    SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0)
            return;

        if (App.Services is null)
            return;

        var vm =
            App.Services.GetService<MainViewModel>();

        if (vm is null)
            return;

        if (e.AddedItems[0] is not Surah surah)
            return;

        try
        {
            Debug.WriteLine(
                "========================================");

            Debug.WriteLine(
                "[TOOLBAR SURAH] Surah dipilih");

            Debug.WriteLine(
                $"Surah = {surah.Id} - {surah.LatinName}");

            // =====================================================
            // 1. MODE AYAT
            // =====================================================

            vm.DisplayViewMode =
                QuranDisplayMode.Ayat;

            // =====================================================
            // 2. PILIH SURAH
            // =====================================================
            //
            // Ini akan memicu:
            //
            // OnSelectedSurahChanged()
            //        ↓
            // HandleSelectedSurahChangedAsync()
            //        ↓
            // LoadAyatAsync()
            //

            vm.SelectedSurah = surah;

            // =====================================================
            // 3. TUNGGU SAMPAI DATA AYAT SELESAI
            // =====================================================
            //
            // Karena OnSelectedSurahChanged() menjalankan
            // async secara fire-and-forget, kita load di sini
            // secara eksplisit supaya kita tahu kapan selesai.
            //

            await vm.LoadAyatAsync(
                surah.Id);

            // =====================================================
            // 4. PILIH AYAT PERTAMA
            // =====================================================

            var firstAyat =
                vm.Ayats.FirstOrDefault();

            if (firstAyat is not null)
            {
                vm.SelectedAyat =
                    firstAyat;
            }

            // =====================================================
            // 5. PINDAH KE READER
            // =====================================================

            ShowView(ReaderView);

            Debug.WriteLine(
                $"[TOOLBAR SURAH] Reader dibuka: " +
                $"{surah.LatinName}");

            Debug.WriteLine(
                $"[TOOLBAR SURAH] " +
                $"Jumlah ayat = {vm.Ayats.Count}");

            Debug.WriteLine(
                "========================================");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                "[TOOLBAR SURAH ERROR]");

            Debug.WriteLine(
                ex.ToString());
        }
    }

    //private async void TestCopyDialogAsync()
    //{
    //    var dialog = new ContentDialog
    //    {
    //        XamlRoot = Content.XamlRoot,
    //        Content = new CopyAyatDialog()
    //    };

    //    await dialog.ShowAsync();
    //}

    //private void TestCopyButton_Click(
    //object sender,
    //RoutedEventArgs e)
    //{
    //    TestCopyDialogAsync();
    //}

    private async void Toolbar_CopyRequested(
    object sender,
    RoutedEventArgs e)
    {
        try
        {
            // ============================================================
            // BUAT COPY CONTROL
            // ============================================================

            var copyControl =
                new CopyAyatDialog();


            // ============================================================
            // BUAT CONTENT DIALOG
            // ============================================================

            var dialog =
                new ContentDialog
                {
                    Title = "",
                    Content = copyControl,
                    XamlRoot = Content.XamlRoot
                };


            // ============================================================
            // TAMPILKAN
            // ============================================================

            await dialog.ShowAsync();


            // ============================================================
            // CEK HASIL
            // ============================================================

            if (!copyControl.IsCopyConfirmed)
            {
                Debug.WriteLine(
                    "[COPY] User membatalkan copy.");

                return;
            }


            // ============================================================
            // HASIL COPY
            // ============================================================

            if (!string.IsNullOrWhiteSpace(
                    copyControl.CopyResult))
            {
                Debug.WriteLine(
                    "========== HASIL COPY ==========");

                Debug.WriteLine(
                    copyControl.CopyResult);

                Debug.WriteLine(
                    "========== AKHIR COPY ==========");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                "========================================");

            Debug.WriteLine(
                "[COPY DIALOG ERROR]");

            Debug.WriteLine(
                ex.ToString());

            Debug.WriteLine(
                "========================================");
        }
    }

}