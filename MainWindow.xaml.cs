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
using System.ComponentModel;


namespace QuranKemenag;

/// <summary>
/// Code-behind MainWindow: hanya bertugas mengikat ViewModel dan memicu pemuatan data.
/// </summary>
public sealed partial class MainWindow : Window
{
    private bool _isInitialized;
    private bool _isSidebarVisible = true;
    private bool _isPanelVisible = true;
    private MainViewModel? _vmRef;

    // Responsive adaptive state for Feature Panel
    private enum AdaptiveState { Wide, Medium, Compact }
    private AdaptiveState _adaptiveState = AdaptiveState.Wide;
    private FrameworkElement? _rootElement;

    public MainWindow()
    {
        this.InitializeComponent();

        //TestMushafLayout();

        Title = "Quran Kemenag for Windows";

        // Do not use DI or ViewModel for this repository test.
        Activated += MainWindow_Activated;
    }

    private void RootElement_SizeChanged(
     object? sender,
     SizeChangedEventArgs e)
    {
        UpdateAdaptiveState(e.NewSize.Width);
    }

    private void UpdateAdaptiveState(double width)
    {
        var newState =
            width >= 1100
                ? AdaptiveState.Wide
                : width >= 700
                    ? AdaptiveState.Medium
                    : AdaptiveState.Compact;

        if (_adaptiveState == newState)
            return;

        _adaptiveState = newState;

        ApplyFeaturePanelResponsiveLayout();
    }

    private void ApplyFeaturePanelResponsiveLayout()
    {
        Toolbar.SetResponsiveState(
            _adaptiveState == AdaptiveState.Wide,
            _adaptiveState == AdaptiveState.Compact);

        switch (_adaptiveState)
        {
            case AdaptiveState.Wide:
                // Feature Panel berada di kanan dan mengambil
                // area kolom Content + Panel.
                PanelColumn.Width = new GridLength(300);

                FeaturePanel.Visibility = Visibility.Visible;
                FeaturePanel.HorizontalAlignment = HorizontalAlignment.Right;
                FeaturePanel.VerticalAlignment = VerticalAlignment.Stretch;
                FeaturePanel.Width = 300;

                Canvas.SetZIndex(FeaturePanel, 10);

                _isPanelVisible = true;
                break;


            case AdaptiveState.Medium:
                // Pada ukuran Medium, panel disembunyikan
                // agar area Reader lebih luas.
                PanelColumn.Width = new GridLength(0);

                FeaturePanel.Visibility = Visibility.Collapsed;

                FeaturePanel.ClearValue(FrameworkElement.WidthProperty);
                FeaturePanel.ClearValue(FrameworkElement.HorizontalAlignmentProperty);
                FeaturePanel.ClearValue(FrameworkElement.VerticalAlignmentProperty);

                Canvas.SetZIndex(FeaturePanel, 0);

                _isPanelVisible = false;
                break;


            case AdaptiveState.Compact:
                // Pada Compact, panel tidak mengambil ruang layout.
                // Panel akan ditampilkan sebagai overlay ketika tombol
                // Feature Panel ditekan.
                PanelColumn.Width = new GridLength(0);

                FeaturePanel.Visibility = Visibility.Collapsed;

                FeaturePanel.Width = 300;
                FeaturePanel.HorizontalAlignment = HorizontalAlignment.Right;
                FeaturePanel.VerticalAlignment = VerticalAlignment.Stretch;

                Canvas.SetZIndex(FeaturePanel, 1000);

                _isPanelVisible = false;
                break;
        }
    }

    private void MainWindow_Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e is null)
            return;

        if (e.PropertyName != "SelectedJuz")
            return;

        // Ensure ReaderView is visible so layout can measure/arrange before
        // the Juz data is populated by LoadJuzAsync in the ViewModel.
        // Keep this minimal and side-effect free (do not trigger any data load).
        try
        {
            if (ReaderView.Visibility != Visibility.Visible)
            {
                ShowView(ReaderView);
            }
        }
        catch
        {
            // no-op: do not rethrow; this is a visibility helper only
        }
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

        BookmarkView.Visibility =
            view == BookmarkView
                ? Visibility.Visible
                : Visibility.Collapsed;

        // Toolbar & Feature Panel hanya aktif saat Reader tampil.
        bool readerActive = view == ReaderView;
        Toolbar.SetReaderActive(readerActive);
        FeaturePanel.SetReaderActive(readerActive);

        if (view == HomeView)
        {
            HomeView.RefreshLastReadingPosition();
        }

        // Expose a simple public way to show the ReaderView from other
        // parts of the UI (e.g. HomeView) without duplicating navigation
        // logic. This keeps ShowView encapsulated while allowing callers
        // to request the Reader be displayed.
    }

    public void ShowReaderView()
    {
        ShowView(ReaderView);
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
                _rootElement = root;

                // Monitor size changes to drive responsive behavior for FeaturePanel
                _rootElement.SizeChanged -= RootElement_SizeChanged;
                _rootElement.SizeChanged += RootElement_SizeChanged;

                // Initialize adaptive state
                UpdateAdaptiveState(_rootElement.ActualWidth);

                // Keep reference and watch for SelectedJuz changes from toolbar
                // so we can ensure ReaderView is shown when user selects a Juz.
                // This is strictly diagnostic/behavioral to ensure layout is
                // available before the Juz data is populated.
                _vmRef = vm;
                // Attach a lightweight handler to ensure ReaderView is made visible
                // immediately when SelectedJuz changes (so layout can measure
                // before LoadJuzAsync fills the reader). This is non-diagnostic
                // and intentionally minimal: it only calls ShowView(ReaderView).
                _vmRef.PropertyChanged -= MainWindow_Vm_PropertyChanged;
                _vmRef.PropertyChanged += MainWindow_Vm_PropertyChanged;

                // Wire sidebar navigation events so selecting 'Page' shows MushafPageView
                // and selecting Surah/Home returns to Ayat view.
                try
                {
                    Sidebar.HomeClicked -= Sidebar_HomeClicked;
                    Sidebar.SurahClicked -= Sidebar_SurahClicked;
                    Sidebar.PageClicked -= Sidebar_PageClicked;
                    Sidebar.BookmarkClicked -= Sidebar_BookmarkClicked;

                    Sidebar.HomeClicked += Sidebar_HomeClicked;
                    Sidebar.SurahClicked += Sidebar_SurahClicked;
                    Sidebar.PageClicked += Sidebar_PageClicked;
                    Sidebar.BookmarkClicked += Sidebar_BookmarkClicked;

                    Sidebar.JuzClicked -= Sidebar_JuzClicked;
                    Sidebar.JuzClicked += Sidebar_JuzClicked;

                    BookmarkView.BookmarkOpened -= BookmarkView_BookmarkOpened;
                    BookmarkView.BookmarkOpened += BookmarkView_BookmarkOpened;

                    BookmarkView.BookmarkDeleted -= BookmarkView_BookmarkDeleted;
                    BookmarkView.BookmarkDeleted += BookmarkView_BookmarkDeleted;

                    // =====================================================
                    // TOOLBAR
                    // =====================================================

                    Toolbar.SidebarToggleRequested -=
                        Toolbar_SidebarToggleRequested;

                    Toolbar.SidebarToggleRequested +=
                        Toolbar_SidebarToggleRequested;

                    Sidebar.SidebarToggleRequested -=
                        Toolbar_SidebarToggleRequested;

                    Sidebar.SidebarToggleRequested +=
                        Toolbar_SidebarToggleRequested;

                    // Event ketika user memilih salah satu surah.
                    SurahView.SurahSelected -= SurahView_SurahSelected;
                    SurahView.SurahSelected += SurahView_SurahSelected;

                    SurahView.JuzRequested -= SurahView_JuzRequested;
                    SurahView.JuzRequested += SurahView_JuzRequested;

                    JuzView.SurahRequested -= JuzView_SurahRequested;
                    JuzView.SurahRequested += JuzView_SurahRequested;

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

                // Ensure initial panel column width matches initial visibility
                PanelColumn.Width = _isPanelVisible ? new GridLength(300) : new GridLength(0);
                }
                catch { /* ignore if Sidebar not available yet */ }
            }

            // Load surah once
            await vm.LoadSurahAsync();

            // Verify count
            Debug.WriteLine($"Jumlah Surah : {vm.Surahs.Count}");

            // Tampilkan Home sebagai halaman awal aplikasi.
            ShowView(HomeView);

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

            Sidebar.SetPaneOpen(true);
        }
        else
        {
            Sidebar.Visibility =
                Visibility.Visible;

            SidebarColumn.Width =
                new GridLength(64);

            Sidebar.SetPaneOpen(false);
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

        // Show existing juz list view
        ShowView(JuzView);
    }

    private async void Sidebar_BookmarkClicked(
    object? sender,
    EventArgs e)
    {
        Debug.WriteLine("[BOOKMARK] Bookmark diklik");

        ShowView(BookmarkView);

        await BookmarkView.RefreshBookmarksAsync();
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
            // Juz selected: JuzNumber={juz.JuzNumber}, Surah={juz.SurahNumber}, Ayat={juz.AyatNumber}


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

            var _rootFe_before = this.Content as FrameworkElement;
            Debug.WriteLine($"[LAYOUT DEBUG] BEFORE ShowView - MainWindow={( _rootFe_before is null ? 0 : _rootFe_before.ActualWidth)}x{( _rootFe_before is null ? 0 : _rootFe_before.ActualHeight)}");
            Debug.WriteLine($"[LAYOUT DEBUG] BEFORE ShowView - ContentHost={ContentHost.ActualWidth}x{ContentHost.ActualHeight}");
            Debug.WriteLine($"[LAYOUT DEBUG] BEFORE ShowView - ReaderView={ReaderView.ActualWidth}x{ReaderView.ActualHeight}");

            ShowView(
                ReaderView);

            var _rootFe_after = this.Content as FrameworkElement;
            Debug.WriteLine($"[LAYOUT DEBUG] AFTER ShowView - MainWindow={( _rootFe_after is null ? 0 : _rootFe_after.ActualWidth)}x{( _rootFe_after is null ? 0 : _rootFe_after.ActualHeight)}");
            Debug.WriteLine($"[LAYOUT DEBUG] AFTER ShowView - ContentHost={ContentHost.ActualWidth}x{ContentHost.ActualHeight}");
            Debug.WriteLine($"[LAYOUT DEBUG] AFTER ShowView - ReaderView={ReaderView.ActualWidth}x{ReaderView.ActualHeight}");


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
        try
        {
            // Adaptive behavior:
            // - Wide: toggle column as before
            // - Medium: toggle column (collapsed by default)
            // - Compact: show as overlay (do not expand PanelColumn)
            if (_adaptiveState == AdaptiveState.Compact)
            {
                // In compact mode, toggle overlay visibility by manipulating
                // the FeaturePanel's Visual properties without changing the Grid column width.
                if (_isPanelVisible)
                {
                    _isPanelVisible = false;

                    FeaturePanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    _isPanelVisible = true;

                    FeaturePanel.Visibility = Visibility.Visible;

                    FeaturePanel.Width = 300;
                    FeaturePanel.HorizontalAlignment = HorizontalAlignment.Right;
                    FeaturePanel.VerticalAlignment = VerticalAlignment.Stretch;

                    Canvas.SetZIndex(FeaturePanel, 1000);
                }
                return;
            }
            else
            {
                // Wide / Medium: existing column toggle behavior
                if (_isPanelVisible)
                {
                    _isPanelVisible = false;

                    FeaturePanel.Visibility = Visibility.Collapsed;
                    PanelColumn.Width = new GridLength(0);
                }
                else
                {
                    _isPanelVisible = true;

                    FeaturePanel.Visibility = Visibility.Visible;
                    PanelColumn.Width = new GridLength(300);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[PANEL TOGGLE ERROR]");
            Debug.WriteLine(ex.ToString());
        }
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

    // agar Toolbar_SurahSelectionRequested tidak dipanggil saat sinkronisasi Juz/Ayat

    //private async void Toolbar_SurahSelectionRequested(
    //object sender,
    //SelectionChangedEventArgs e)
    //{
    //    if (e.AddedItems.Count == 0)
    //        return;

    //    if (App.Services is null)
    //        return;

    //    var vm =
    //        App.Services.GetService<MainViewModel>();

    //    if (vm is null)
    //        return;

    //    if (e.AddedItems[0] is not Surah surah)
    //        return;

    //    try
    //    {
    //        Debug.WriteLine(
    //            "========================================");

    //        Debug.WriteLine(
    //            "[TOOLBAR SURAH] Surah dipilih");

    //        Debug.WriteLine(
    //            $"Surah = {surah.Id} - {surah.LatinName}");

    //        // =====================================================
    //        // 1. MODE AYAT
    //        // =====================================================

    //        // If the ViewModel is currently loading ayat (e.g. during Juz load),
    //        // ignore toolbar selection changes that originate from programmatic
    //        // updates to SelectedSurah. This prevents the UI handler from
    //        // forcing a full surah load and overwriting the Juz context.
    //        if (vm.IsLoadingAyat)
    //        {
    //            Debug.WriteLine(
    //                "[TOOLBAR SURAH] Ignored SelectionChanged while IsLoadingAyat=true.");
    //            return;
    //        }

    //        vm.DisplayViewMode =
    //            QuranDisplayMode.Ayat;

    //        // =====================================================
    //        // 2. PILIH SURAH
    //        // =====================================================
    //        //
    //        // Ini akan memicu:
    //        //
    //        // OnSelectedSurahChanged()
    //        //        ↓
    //        // HandleSelectedSurahChangedAsync()
    //        //        ↓
    //        // LoadAyatAsync()
    //        //

    //        vm.SelectedSurah = surah;

    //        // =====================================================
    //        // 3. TUNGGU SAMPAI DATA AYAT SELESAI
    //        // =====================================================
    //        //
    //        // Karena OnSelectedSurahChanged() menjalankan
    //        // async secara fire-and-forget, kita load di sini
    //        // secara eksplisit supaya kita tahu kapan selesai.
    //        //

    //        // Await the async handler started by the ViewModel when SelectedSurah
    //        // changed. This prevents invoking LoadAyatAsync twice (once via
    //        // the handler and once explicitly) which caused duplicate loads.
    //        await (vm.LastHandleSelectedSurahTask ?? Task.CompletedTask);

    //        // =====================================================
    //        // 4. PILIH AYAT PERTAMA
    //        // =====================================================

    //        var firstAyat =
    //            vm.Ayats.FirstOrDefault();

    //        if (firstAyat is not null)
    //        {
    //            vm.SelectedAyat =
    //                firstAyat;
    //        }

    //        // =====================================================
    //        // 5. PINDAH KE READER
    //        // =====================================================

    //        var _rootFe_before = this.Content as FrameworkElement;
    //        Debug.WriteLine($"[LAYOUT DEBUG] BEFORE ShowView - MainWindow={(_rootFe_before is null ? 0 : _rootFe_before.ActualWidth)}x{(_rootFe_before is null ? 0 : _rootFe_before.ActualHeight)}");
    //        Debug.WriteLine($"[LAYOUT DEBUG] BEFORE ShowView - ContentHost={ContentHost.ActualWidth}x{ContentHost.ActualHeight}");
    //        Debug.WriteLine($"[LAYOUT DEBUG] BEFORE ShowView - ReaderView={ReaderView.ActualWidth}x{ReaderView.ActualHeight}");

    //        ShowView(ReaderView);

    //        var _rootFe_after = this.Content as FrameworkElement;
    //        Debug.WriteLine($"[LAYOUT DEBUG] AFTER ShowView - MainWindow={(_rootFe_after is null ? 0 : _rootFe_after.ActualWidth)}x{(_rootFe_after is null ? 0 : _rootFe_after.ActualHeight)}");
    //        Debug.WriteLine($"[LAYOUT DEBUG] AFTER ShowView - ContentHost={ContentHost.ActualWidth}x{ContentHost.ActualHeight}");
    //        Debug.WriteLine($"[LAYOUT DEBUG] AFTER ShowView - ReaderView={ReaderView.ActualWidth}x{ReaderView.ActualHeight}");

    //        Debug.WriteLine(
    //            $"[TOOLBAR SURAH] Reader dibuka: " +
    //            $"{surah.LatinName}");

    //        Debug.WriteLine(
    //            $"[TOOLBAR SURAH] " +
    //            $"Jumlah ayat = {vm.Ayats.Count}");

    //        Debug.WriteLine(
    //            "========================================");
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.WriteLine(
    //            "[TOOLBAR SURAH ERROR]");

    //        Debug.WriteLine(
    //            ex.ToString());
    //    }
    //}

    //edit mulai
    private async void Toolbar_SurahSelectionRequested(
    object sender,
    SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0)
            return;

        if (App.Services is null)
            return;

        var vm = App.Services.GetService<MainViewModel>();
        if (vm is null)
            return;

        if (e.AddedItems[0] is not Surah surah)
            return;

        try
        {
            // 1. Langsung pindah ke Reader supaya user tidak melihat layar diam.
            //    (Loading text sudah ada lewat IsLoadingAyat/LoadingAyatText.)
            ShowView(ReaderView);

            // 2. Pastikan mode Ayat
            vm.DisplayViewMode = QuranDisplayMode.Ayat;

            // 3. Binding TwoWay biasanya sudah mengisi SelectedSurah.
            //    Jika belum, set manual agar handler VM berjalan.
            if (vm.SelectedSurah?.Id != surah.Id)
                vm.SelectedSurah = surah;

            // 4. Tunggu load ayat selesai
            await (vm.LastHandleSelectedSurahTask ?? Task.CompletedTask);

            // 5. Pilih ayat pertama
            var firstAyat = vm.Ayats.FirstOrDefault();
            if (firstAyat is not null)
                vm.SelectedAyat = firstAyat;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[TOOLBAR SURAH ERROR]");
            Debug.WriteLine(ex);
        }
    }
    //edit selesai

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

    private void SurahView_JuzRequested(
    object? sender,
    EventArgs e)
    {
        if (App.Services is null)
            return;

        var vm =
            App.Services.GetService<MainViewModel>();

        if (vm is null)
            return;

        // Bersihkan context Reader sebelumnya
        vm.SelectedSurah = null;
        vm.SelectedAyat = null;

        Debug.WriteLine(
            "[SURAH VIEW] Tab Juz diklik");

        ShowView(JuzView);
    }

    private void JuzView_SurahRequested(
    object? sender,
    EventArgs e)
    {
        Debug.WriteLine(
            "[JUZ VIEW] Tab Surah diklik");

        ShowView(SurahView);
    }

    private async void BookmarkView_BookmarkOpened(
    object? sender,
    Bookmark bookmark)
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
                $"[BOOKMARK NAVIGATION] " +
                $"Surah={bookmark.Surah}, " +
                $"Ayat={bookmark.Ayat}");

            // Cari Surah berdasarkan nomor Surah
            var surah =
                vm.Surahs.FirstOrDefault(
                    s => s.Id == bookmark.Surah);

            if (surah is null)
                return;

            // Mode Ayat
            vm.DisplayViewMode =
                QuranDisplayMode.Ayat;

            // Pilih Surah.
            // Ini menggunakan mekanisme navigasi Surah yang sudah ada.
            vm.SelectedSurah = surah;

            // Tunggu proses Load Surah yang sudah dijalankan
            // oleh OnSelectedSurahChanged().
            await (vm.LastHandleSelectedSurahTask
                ?? Task.CompletedTask);

            // Cari ayat yang diminta dari daftar ayat Surah.
            var ayat =
                vm.Ayats.FirstOrDefault(
                    a => a.Surah == bookmark.Surah &&
                         a.AyatNumber == bookmark.Ayat);

            if (ayat is null)
                return;

            // Pilih ayat.
            // ReaderView yang sudah ada akan menangani
            // scroll dan highlight.
            vm.SelectedAyat = ayat;

            // Tampilkan Reader.
            ShowView(ReaderView);

            Debug.WriteLine(
                $"[BOOKMARK NAVIGATION] Reader dibuka: " +
                $"Surah={surah.LatinName}, " +
                $"Ayat={ayat.AyatNumber}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                "[BOOKMARK NAVIGATION ERROR]");

            Debug.WriteLine(ex);
        }
    }

    private async void BookmarkView_BookmarkDeleted(
    object? sender,
    Bookmark bookmark)
    {
        try
        {
            await ReaderView.RefreshBookmarkStatusAsync(
                bookmark.Surah,
                bookmark.Ayat);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                "[BOOKMARK DELETE SYNC ERROR]");

            Debug.WriteLine(ex);
        }
    }

}