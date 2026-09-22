using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using QuranKemenag.Constants;
using QuranKemenag.Helpers;
using QuranKemenag.Models;
using QuranKemenag.Repositories;
using QuranKemenag.Services;
using QuranKemenag.Services.Logging;
using QuranKemenag.Services.Page;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;

namespace QuranKemenag.ViewModels;

/// <summary>
/// ViewModel utama aplikasi Quran Kemenag.
///
/// Tanggung jawab:
/// 1. Memuat daftar Surah.
/// 2. Memuat ayat.
/// 3. Mengatur mode reader Unicode / Symbol.
/// 4. Mengatur mode tampilan Ayat / Halaman.
/// 5. Mengatur navigasi ayat.
/// 6. Mengatur navigasi halaman mushaf.
/// 7. Mengatur sinkronisasi SelectedAyat dan CurrentPage.
/// 8. Mengatur ukuran tampilan dan preset.
/// </summary>
public sealed partial class MainViewModel : ViewModelBase
{
    #region Fields

    private readonly IQuranRepository _repository;

    private readonly ITranslationRepository _translationRepository;

    private readonly ReaderContext _readerContext;

    private readonly ILogService _logService;

    private readonly IQuranPageService _quranPageService;

    /// <summary>
    /// ID request terakhir.
    /// Digunakan agar hasil load Surah lama
    /// tidak menimpa hasil request terbaru.
    /// </summary>
    private long _loadAyatRequestId;

    /// <summary>
    /// Halaman yang sedang tersedia dalam context reader.
    ///
    /// Saat sebuah Surah dimuat:
    /// berisi halaman yang dimiliki Surah tersebut.
    ///
    /// Saat navigasi halaman global:
    /// dapat berisi halaman aktif yang sedang ditampilkan.
    /// </summary>
    private IReadOnlyList<QuranPage> _pages =
        Array.Empty<QuranPage>();

    /// <summary>
    /// Halaman mushaf yang sedang aktif.
    /// </summary>
    private QuranPage? _currentPage;

    /// <summary>
    /// Preset tampilan yang aktif.
    /// </summary>
    private string? _selectedDisplayPreset = "Standar";

    // Menandai bahwa perubahan SelectedSurah
    // Sedang terjadi sebagai bagian dari proses membuka Juz
    private bool _isLoadingJuz;

    private int? _activeJuzNumber;
    // Pending navigation request when user selects an Ayat that belongs to a different Juz.
    // Stored as (Surah, AyatNumber, JuzNumber) and consumed by LoadJuzAsync after the juz is loaded.
    private (int Surah, int AyatNumber, int Juz)? _pendingNavigation;
    // Suppress calling LoadJuzAsync when SelectedJuz is changed programmatically
    // as part of SelectedAyat synchronization.
    private bool _suppressSelectedJuzChanged;

    // UI dispatcher captured at construction time to ensure
    // ObservableCollection updates are performed on the UI thread.
    private readonly Microsoft.UI.Dispatching.DispatcherQueue _uiDispatcher;

    #endregion

    // Load full list of ayat for a specific surah for toolbar navigation.
    private async Task LoadSurahAyatsAsync(int surahNumber)
    {
        try
        {
            // Capture UI dispatcher BEFORE awaiting the repository call so
            // we retain a valid UI dispatcher instance for enqueuing updates.
            var dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

            var list = await _readerContext.GetAyatAsync(surahNumber);
            if (list is null)
                return;

            // Ensure the method completes only after the UI dispatcher has
            // applied the collection updates. This prevents a race where
            // SelectedValue is applied before the ComboBox ItemsSource
            // has been populated, which would leave the ComboBox showing
            // its placeholder.
            if (dispatcher is null)
            {
                // If we do not have a dispatcher for the current thread then
                // attempt to enqueue on the thread captured by GetForCurrentThread
                // earlier is not possible; fall back to direct update (rare).
                SurahAyats.Clear();
                foreach (var a in list)
                    SurahAyats.Add(a);
                return;
            }

            var tcs = new TaskCompletionSource<int>();

            dispatcher.TryEnqueue(() =>
            {
                try
                {
                    SurahAyats.Clear();
                    foreach (var a in list)
                        SurahAyats.Add(a);

                    tcs.SetResult(0);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            // Await the dispatcher action completion so callers know when
            // the collection has been populated.
            await tcs.Task.ConfigureAwait(false);
        }
        catch
        {
            // suppressed temporary diagnostic
        }
    }

    /// <summary>
    /// Enrich existing JuzList with detailed information from JuzRepository.
    /// This method updates individual items in the existing ObservableCollection
    /// so bindings remain intact. Exceptions are caught and logged.
    /// </summary>
    private async Task LoadJuzListInfoAsync()
    {
        try
        {
            if (App.Services is null)
                return;

            var repo = App.Services.GetRequiredService<JuzRepository>();

            // Capture UI thread dispatcher before awaiting repository call
            var dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

            var list = await repo.GetAllJuzInfoAsync().ConfigureAwait(false);

            if (list is null || list.Count == 0)
                return;

            // Preserve currently selected Juz number so we can restore selection
            // after updating collection items.
            int? currentlySelected = SelectedJuz?.JuzNumber;

            // Update each existing entry in JuzList with data from repository.
                foreach (var info in list)
                {
                    var existing = JuzList.FirstOrDefault(x => x.JuzNumber == info.JuzNumber);
                    if (existing is not null)
                    {
                        var idx = JuzList.IndexOf(existing);

                        // Create new instance so ObservableCollection raises Replace.
                        var updated = new JuzInfo
                        {
                            JuzNumber = info.JuzNumber,
                            SurahNumber = info.SurahNumber,
                            AyatNumber = info.AyatNumber,
                            SurahName = info.SurahName
                        };

                    // Replace item at index to notify UI bindings.
                    dispatcher?.TryEnqueue(() => JuzList[idx] = updated);
                }
            }

        // Restore SelectedJuz if it was set previously
            if (currentlySelected is int sel)
            {
                // Find the updated item in JuzList
                var newSelected = JuzList.FirstOrDefault(x => x.JuzNumber == sel);
                if (newSelected is not null)
                {
                    // Suppress SelectedJuz change handler to avoid triggering LoadJuzAsync
                    _suppressSelectedJuzChanged = true;
                    dispatcher?.TryEnqueue(() => SelectedJuz = newSelected);
                    _suppressSelectedJuzChanged = false;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[JUZ LIST LOAD ERROR]");
            Debug.WriteLine(ex.ToString());
        }
    }


    #region Collections

    /// <summary>
    /// Daftar seluruh Surah.
    /// </summary>
    public ObservableCollection<Surah> Surahs { get; } = new();

    /// <summary>
    /// Daftar mode reader:
    /// Computer Font / Handwritten Manuscript.
    /// </summary>
    public ObservableCollection<DisplayModeItem> DisplayModes { get; } = new();

    /// <summary>
    /// Daftar ayat yang sedang dimuat.
    /// </summary>
    public ObservableCollection<Ayat> Ayats { get; } = new();

    /// <summary>
    /// Full list of Ayats for the currently selected Surah (used for navigation ComboBox).
    /// This is separate from the Reader "Ayats" collection which may represent a Juz context.
    /// </summary>
    public ObservableCollection<Ayat> SurahAyats { get; } = new();

    /// <summary>
    /// Daftar preset tampilan.
    /// </summary>
    public ObservableCollection<string> DisplayPresets { get; } =
        new()
        {
            "Standar",
            "Nyaman",
            "Besar"
        };

    /// <summary>
    /// Daftar mode tampilan Quran.
    /// </summary>
    public IEnumerable<QuranDisplayMode> QuranDisplayModes { get; } =
        Enum.GetValues<QuranDisplayMode>();

    /// <summary>
    /// Daftar Juz (1..30) untuk binding di Toolbar.
    /// </summary>
    public ObservableCollection<JuzInfo> JuzList { get; } = new();

    #endregion

    [ObservableProperty]
    private int? selectedAyatNumber;


    #region Observable Properties

    /// <summary>
    /// Mode reader yang dipilih:
    /// Computer Font atau Handwritten Manuscript.
    /// </summary>
    [ObservableProperty]
    private DisplayModeItem? selectedDisplayModeItem;

    /// <summary>
    /// Surah yang sedang dipilih.
    /// </summary>
    [ObservableProperty]
    private Surah? selectedSurah;

    /// <summary>
    /// Ayat yang sedang dipilih.
    /// </summary>
    [ObservableProperty]
    private Ayat? selectedAyat;

    /// <summary>
    /// Menentukan apakah terjemahan ditampilkan.
    /// </summary>
    [ObservableProperty]
    private bool showTranslation = true;

    /// <summary>
    /// Menentukan apakah Bahasa Isyarat ditampilkan.
    /// </summary>
    [ObservableProperty]
    private bool showIsyarat = true;

    /// <summary>
    /// Ukuran font Arab.
    /// </summary>
    [ObservableProperty]
    private double arabicFontSize = 42;

    /// <summary>
    /// Ukuran font terjemahan.
    /// </summary>
    [ObservableProperty]
    private double translationFontSize = 18;

    /// <summary>
    /// Ukuran font Bahasa Isyarat.
    /// </summary>
    [ObservableProperty]
    private double isyaratFontSize = 22;

    /// <summary>
    /// Menampilkan indikator loading ayat.
    /// </summary>
    [ObservableProperty]
    private bool isLoadingAyat;

    /// <summary>
    /// Teks loading.
    /// </summary>
    [ObservableProperty]
    private string loadingAyatText = string.Empty;

    /// <summary>
    /// Posisi ayat atau halaman.
    /// </summary>
    [ObservableProperty]
    private string currentAyatPosition = string.Empty;

    /// <summary>
    /// Mode tampilan utama:
    ///
    /// Ayat
    /// atau
    /// Halaman.
    /// </summary>
    [ObservableProperty]
    private QuranDisplayMode displayViewMode =
        QuranDisplayMode.Ayat;

    /// <summary>
    /// Jumlah seluruh halaman Quran.
    ///
    /// Nilai ini digunakan untuk navigasi halaman global.
    /// </summary>
    [ObservableProperty]
    private int totalQuranPages;

    /// <summary>
    /// //Juz yang sedang dipilih.
    /// </summary>
    [ObservableProperty]
    private JuzInfo? selectedJuz;

    /// <summary>
    /// Called when SelectedJuz changes (e.g. user picks a Juz from toolbar).
    /// Triggers loading of the selected Juz unless the change originates
    /// from an ongoing Juz load (guarded by _isLoadingJuz) or the Juz
    /// is already active.
    /// </summary>
    partial void OnSelectedJuzChanged(JuzInfo? value)
    {
        // If this change was initiated by SelectedAyat synchronization,
        // do not react (avoid triggering LoadJuzAsync).
        if (_suppressSelectedJuzChanged)
        {
            return;
        }
        try
        {
            if (value is null)
                return;

            // If we're in the middle of loading a Juz, ignore programmatic
            // updates coming from LoadJuzAsync itself.
            if (_isLoadingJuz)
            {
                // Ignore programmatic updates while a juz load is in progress.
                return;
            }

            // If the selected juz is already the active one, do nothing.
            if (_activeJuzNumber is int active && active == value.JuzNumber)
            {
                return;
            }

            // Ensure reader shows Ayat list when user selects a Juz from toolbar.
            // This must be done before loading so the UI presents the full
            // Juz context rather than a single page when DisplayViewMode
            // was previously set to Halaman.
            DisplayViewMode = QuranDisplayMode.Ayat;

            // Fire-and-forget the async load. LoadJuzAsync contains its own
            // request-id guard to avoid races.
            _ = LoadJuzAsync(value.JuzNumber);
        }
        catch (Exception ex)
        {
            // suppressed temporary diagnostic
        }
    }



    #endregion


    #region Display Preset

    /// <summary>
    /// Preset tampilan yang sedang dipilih.
    /// </summary>
    public string? SelectedDisplayPreset
    {
        get => _selectedDisplayPreset;

        set
        {
            if (_selectedDisplayPreset == value)
                return;

            _selectedDisplayPreset = value;

            OnPropertyChanged();

            ApplyDisplayPreset(value);
        }
    }

    /// <summary>
    /// Menerapkan preset ukuran tampilan.
    /// </summary>
    public void ApplyDisplayPreset(string? preset)
    {
        Debug.WriteLine(
            $"[PRESET] Selected = {preset}");

        switch (preset)
        {
            case "Standar":

                ArabicFontSize = 42;
                TranslationFontSize = 18;
                IsyaratFontSize = 22;

                ShowTranslation = true;
                ShowIsyarat = true;

                break;

            case "Nyaman":

                ArabicFontSize = 48;
                TranslationFontSize = 20;
                IsyaratFontSize = 24;

                ShowTranslation = true;
                ShowIsyarat = true;

                break;

            case "Besar":

                ArabicFontSize = 56;
                TranslationFontSize = 22;
                IsyaratFontSize = 26;

                ShowTranslation = true;
                ShowIsyarat = true;

                break;
        }

        Debug.WriteLine(
            $"[PRESET] Arabic={ArabicFontSize}, " +
            $"Translation={TranslationFontSize}, " +
            $"Isyarat={IsyaratFontSize}");
    }

    #endregion


    #region Display Mode Ayat / Halaman

    /// <summary>
    /// True jika tampilan menggunakan Mushaf halaman.
    /// </summary>
    public bool IsPageView =>
        DisplayViewMode == QuranDisplayMode.Halaman;

    /// <summary>
    /// True jika tampilan menggunakan daftar ayat.
    /// </summary>
    public bool IsAyatView =>
        DisplayViewMode == QuranDisplayMode.Ayat;

    /// <summary>
    /// Dipanggil otomatis oleh CommunityToolkit
    /// ketika DisplayViewMode berubah.
    /// </summary>
    partial void OnDisplayViewModeChanged(
        QuranDisplayMode value)
    {
        Debug.WriteLine("[DISPLAY DEBUG] Before mode change:");
        Debug.WriteLine($"Mode = {DisplayViewMode}");
        Debug.WriteLine($"IsAyatView = {IsAyatView}");
        Debug.WriteLine($"Ayats.Count = {Ayats.Count}");
        Debug.WriteLine($"CurrentPage = {(CurrentPage is null ? "NULL" : "NOT NULL")}");

        OnPropertyChanged(nameof(IsPageView));
        OnPropertyChanged(nameof(IsAyatView));

        Debug.WriteLine(
            $"[DISPLAY MODE] Mode = {value}");

        if (value == QuranDisplayMode.Halaman)
        {
            /*
             * User masuk ke Mode Halaman.
             *
             * Cari halaman berdasarkan ayat yang sedang dipilih.
             */
            _ = OpenSelectedAyatPageAsync();
        }
        else
        {
            /*
             * User kembali ke Mode Ayat.
             *
             * Ayat aktif mengikuti ayat pertama
             * dari halaman yang sedang dibuka.
             */
            SyncAyatWithCurrentPage();
        }

        UpdateAyatPosition();

        NotifyNavigationCommands();

        Debug.WriteLine("[DISPLAY DEBUG] After mode change:");
        Debug.WriteLine($"Mode = {DisplayViewMode}");
        Debug.WriteLine($"IsAyatView = {IsAyatView}");
        Debug.WriteLine($"Ayats.Count = {Ayats.Count}");
        Debug.WriteLine($"CurrentPage = {(CurrentPage is null ? "NULL" : "NOT NULL")}");
    }

    /// <summary>
    /// Toggle Ayat <-> Halaman.
    /// </summary>
    [RelayCommand]
    private void TogglePageView()
    {
        DisplayViewMode =
            DisplayViewMode == QuranDisplayMode.Ayat
                ? QuranDisplayMode.Halaman
                : QuranDisplayMode.Ayat;
    }

    #endregion


    #region Page State

    /// <summary>
    /// Daftar halaman dalam context saat ini.
    /// </summary>
    public IReadOnlyList<QuranPage> Pages
    {
        get => _pages;

        private set
        {
            if (ReferenceEquals(_pages, value))
                return;

            _pages = value;

            OnPropertyChanged(nameof(Pages));

            NotifyNavigationCommands();
        }
    }

    /// <summary>
    /// Halaman mushaf yang sedang aktif.
    /// </summary>
    public QuranPage? CurrentPage
    {
        get => _currentPage;

        private set
        {
            if (ReferenceEquals(_currentPage, value))
                return;

            _currentPage = value;

            OnPropertyChanged(nameof(CurrentPage));
            OnPropertyChanged(nameof(CurrentPageNumber));
            OnPropertyChanged(nameof(HasPreviousPage));
            OnPropertyChanged(nameof(HasNextPage));

            UpdateAyatPosition();

            /*
             * Jika sedang Mode Halaman,
             * SelectedAyat mengikuti ayat pertama
             * dari halaman aktif.
             */
            if (DisplayViewMode == QuranDisplayMode.Halaman &&
                value?.Ayats is { Count: > 0 })
            {
                var firstAyat =
                    value.Ayats.FirstOrDefault();

                if (firstAyat is not null &&
                    !ReferenceEquals(
                        SelectedAyat,
                        firstAyat))
                {
                    SelectedAyat = firstAyat;
                }
            }

            NotifyNavigationCommands();

            Debug.WriteLine(
                $"[PAGE] CurrentPage = " +
                $"{CurrentPage?.PageNumber}");
        }
    }

    /// <summary>
    /// Nomor halaman aktif.
    /// </summary>
    public int CurrentPageNumber =>
        CurrentPage?.PageNumber ?? 0;

    /// <summary>
    /// Apakah tersedia halaman sebelumnya.
    /// </summary>
    public bool HasPreviousPage =>
        CurrentPageNumber > 1;

    /// <summary>
    /// Apakah tersedia halaman berikutnya.
    /// </summary>
    public bool HasNextPage =>
        CurrentPageNumber > 0 &&
        CurrentPageNumber < TotalQuranPages;

    #endregion


    #region Constructor

    public MainViewModel(
        IQuranRepository repository,
        ReaderContext readerContext,
        ILogService logService,
        ITranslationRepository translationRepository,
        IQuranPageService quranPageService)
    {
        _repository =
            repository
            ?? throw new ArgumentNullException(
                nameof(repository));

        _readerContext =
            readerContext
            ?? throw new ArgumentNullException(
                nameof(readerContext));

        _logService =
            logService
            ?? throw new ArgumentNullException(
                nameof(logService));

        _translationRepository =
            translationRepository
            ?? throw new ArgumentNullException(
                nameof(translationRepository));

        _quranPageService =
            quranPageService
            ?? throw new ArgumentNullException(
                nameof(quranPageService));

        // Capture the UI dispatcher for later use when updating UI-bound collections
        _uiDispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        InitializeDisplayModes();

        CurrentReaderMode =
            ReaderMode.Unicode;

        _readerContext.SetMode(
            CurrentReaderMode);

        // Populate JuzList with Juz 1..30 for toolbar binding (fallback).
        for (int i = 1; i <= 30; i++)
        {
            JuzList.Add(new JuzInfo { JuzNumber = i });
        }

        // Fire-and-forget enrichment of JuzList from repository
        // (does not replace the collection; updates items so bindings remain).
        _ = LoadJuzListInfoAsync();
    }

    #endregion


    #region Reader Mode

    /// <summary>
    /// Mode reader aktif.
    ///
    /// Unicode:
    /// Computer Font.
    ///
    /// Symbol:
    /// Handwritten Manuscript.
    /// </summary>
    public ReaderMode CurrentReaderMode
    {
        get;
        private set;
    } = ReaderMode.Unicode;

    /// <summary>
    /// Mengisi daftar mode reader.
    /// </summary>
    private void InitializeDisplayModes()
    {
        DisplayModes.Clear();

        foreach (var mode in DisplayModeProvider.GetAll())
        {
            DisplayModes.Add(mode);
        }

        SelectedDisplayModeItem =
            DisplayModes.FirstOrDefault();
    }

    /// <summary>
    /// Mengubah DisplayMode UI menjadi ReaderMode.
    /// </summary>
    private static ReaderMode ConvertToReaderMode(
        DisplayMode displayMode)
    {
        return displayMode switch
        {
            DisplayMode.ComputerFont =>
                ReaderMode.Unicode,

            DisplayMode.HandwrittenManuscript =>
                ReaderMode.Symbol,

            _ =>
                ReaderMode.Unicode
        };
    }

    /// <summary>
    /// Dipanggil ketika mode reader Unicode/Symbol berubah.
    /// </summary>
    partial void OnSelectedDisplayModeItemChanged(
        DisplayModeItem? value)
    {
        if (value is null)
            return;

        var oldMode =
            CurrentReaderMode;

        var newMode =
            ConvertToReaderMode(value.Mode);

        if (oldMode == newMode)
            return;

        CurrentReaderMode =
            newMode;

        _readerContext.SetMode(
            CurrentReaderMode);

        LogDisplayMode(
            oldMode,
            CurrentReaderMode);

        // =====================================================
        // PENTING:
        // Context aktif menentukan cara reload data
        // ketika Reader Mode berubah.
        //
        // - Jika _activeJuzNumber terisi,
        //   context AKTIF adalah Juz.
        //   SelectedSurah pada kondisi ini hanya
        //   informasi Surah dari ayat aktif,
        //   BUKAN context utama, sehingga TIDAK BOLEH
        //   dipakai untuk memutuskan cara reload.
        //
        // - Jika _activeJuzNumber kosong,
        //   context aktif adalah Surah biasa.
        // =====================================================

        if (_activeJuzNumber is int activeJuz)
        {
            _ = ReloadJuzAfterDisplayModeChangedAsync(activeJuz);
        }
        else if (SelectedSurah is not null)
        {
            _ = ReloadSurahAfterDisplayModeChangedAsync(SelectedSurah.Id);
        }
    }

    /// <summary>
    /// Mencari instance Ayat baru yang merepresentasikan
    /// posisi Surah + Nomor Ayat yang sama dengan sebelumnya.
    ///
    /// Instance Ayat lama tidak bisa dipakai langsung setelah
    /// reload karena data selalu dimuat ulang sebagai objek baru.
    /// </summary>
    private static Ayat? FindAyatByPosition(
        IReadOnlyList<Ayat> ayats,
        int surahNumber,
        int ayatNumber)
    {
        return ayats.FirstOrDefault(
            x => x.Surah == surahNumber &&
                 x.AyatNumber == ayatNumber);
    }

    /// <summary>
    /// Reload seluruh ayat Surah yang sedang aktif
    /// setelah Reader Mode berubah, sambil mempertahankan
    /// posisi ayat yang sedang dibaca.
    /// </summary>
    private async Task ReloadSurahAfterDisplayModeChangedAsync(
        int surahNumber)
    {
        try
        {
            Debug.WriteLine(
                "========================================");

            Debug.WriteLine(
                "[FONT MODE] Reload Surah");

            Debug.WriteLine(
                $"[FONT MODE] Surah = {surahNumber}");

            Debug.WriteLine(
                $"[FONT MODE] Reader Mode = {CurrentReaderMode}");

            // =====================================================
            // 0. SIMPAN POSISI AYAT SAAT INI
            // =====================================================

            var previousSurah =
                SelectedAyat?.Surah ?? surahNumber;

            var previousAyatNumber =
                SelectedAyat?.AyatNumber;

            // =====================================================
            // 1. LOAD ULANG DATA SURAH
            //
            // LoadAyatAsync sudah menggunakan request ID
            // (_loadAyatRequestId) sehingga request lama
            // otomatis tidak akan menimpa request terbaru.
            // =====================================================

            await LoadAyatAsync(
                surahNumber);

            /*
             * Snapshot request ID setelah LoadAyatAsync selesai.
             * Jika ada request lain (mis. Juz/Surah lain, atau
             * perubahan Reader Mode berikutnya) yang mulai berjalan
             * setelah titik ini, jangan timpa hasilnya.
             */
            var snapshotRequestId =
                _loadAyatRequestId;

            // =====================================================
            // 2. RESTORE POSISI AYAT
            //
            // Instance Ayat lama sudah tidak valid setelah
            // reload, sehingga dicari ulang berdasarkan
            // Surah + Nomor Ayat yang sama.
            //
            // Jika tidak ditemukan, gunakan ayat pertama Surah.
            // =====================================================

            var restoredAyat =
                previousAyatNumber is int number
                    ? FindAyatByPosition(
                        Ayats,
                        previousSurah,
                        number)
                    : null;

            var targetAyat =
                restoredAyat
                ?? Ayats.FirstOrDefault();

            if (targetAyat is null)
            {
                Debug.WriteLine(
                    "[FONT MODE] Tidak ada ayat untuk direstore.");

                return;
            }

            if (snapshotRequestId != _loadAyatRequestId)
            {
                Debug.WriteLine(
                    "[FONT MODE] Request Surah sudah usang, " +
                    "restore posisi dibatalkan.");

                return;
            }

            SelectedAyat =
                targetAyat;

            // =====================================================
            // 3. JIKA MODE HALAMAN
            //    BUKA HALAMAN SESUAI AYAT AKTIF
            // =====================================================

            if (DisplayViewMode ==
                QuranDisplayMode.Halaman)
            {
                await OpenSelectedAyatPageAsync();
            }

            // =====================================================
            // 4. UPDATE UI
            // =====================================================

            UpdateAyatPosition();

            NotifyNavigationCommands();

            Debug.WriteLine(
                $"[FONT MODE] Ayat aktif = " +
                $"{targetAyat.Surah}:{targetAyat.AyatNumber}");

            Debug.WriteLine(
                "========================================");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                "[FONT MODE ERROR]");

            Debug.WriteLine(
                ex.ToString());
        }
    }

    /// <summary>
    /// Reload seluruh ayat Juz yang sedang aktif
    /// setelah Reader Mode berubah, sambil mempertahankan
    /// posisi ayat yang sedang dibaca.
    ///
    /// Menggunakan GetAyatByJuzAsync() (BUKAN LoadAyatAsync)
    /// agar context Juz tidak digantikan oleh context Surah.
    /// </summary>
    private async Task ReloadJuzAfterDisplayModeChangedAsync(
        int juzNumber)
    {
        var requestId =
            Interlocked.Increment(
                ref _loadAyatRequestId);

        try
        {
            Debug.WriteLine(
                "========================================");

            Debug.WriteLine(
                "[FONT MODE] Reload Juz");

            Debug.WriteLine(
                $"[FONT MODE] Juz = {juzNumber}");

            Debug.WriteLine(
                $"[FONT MODE] Reader Mode = {CurrentReaderMode}");

            IsLoadingAyat = true;

            // =====================================================
            // 0. SIMPAN POSISI AYAT SAAT INI
            // =====================================================

            var previousSurah =
                SelectedAyat?.Surah;

            var previousAyatNumber =
                SelectedAyat?.AyatNumber;

            // =====================================================
            // 1. LOAD ULANG SELURUH AYAT JUZ
            // =====================================================

            var ayats =
                await _readerContext
                    .GetAyatByJuzAsync(juzNumber);

            /*
             * Pastikan request masih terbaru.
             * Mencegah request lama (mode sebelumnya)
             * menimpa hasil request terbaru.
             */
            if (requestId != _loadAyatRequestId)
            {
                Debug.WriteLine(
                    "[FONT MODE] Request Juz sudah usang, " +
                    "hasil diabaikan.");

                return;
            }

            if (ayats == null || ayats.Count == 0)
            {
                Debug.WriteLine(
                    $"[FONT MODE] Tidak ada ayat untuk Juz {juzNumber} " +
                    $"pada mode {CurrentReaderMode}.");

                // =================================================
                // Database untuk mode ini (mis. msi_simbol.db)
                // belum memiliki data untuk Juz ini sama sekali.
                //
                // JANGAN biarkan data lama (dari mode sebelumnya)
                // tetap tampil di layar — itu sama saja mencampur
                // data antar mode. Kosongkan tampilan agar jujur
                // menunjukkan data belum tersedia.
                //
                // SelectedSurah = null otomatis mengosongkan
                // Ayats, SelectedAyat, Pages, dan CurrentPage
                // lewat OnSelectedSurahChanged.
                // =================================================

                SelectedSurah = null;

                NotifyNavigationCommands();

                return;
            }

            await MergeJuzTranslationsAsync(ayats);

            if (requestId != _loadAyatRequestId)
            {
                Debug.WriteLine(
                    "[FONT MODE] Request Juz sudah usang setelah " +
                    "translation, hasil diabaikan.");

                return;
            }

            // =====================================================
            // 2. SIMPAN SELURUH AYAT JUZ KE COLLECTION
            //
            // _isLoadingJuz dipakai agar perubahan SelectedSurah
            // di bawah tidak memicu LoadAyatAsync() dan
            // menggantikan context Juz yang baru dimuat.
            // =====================================================

            _isLoadingJuz = true;

            try
            {
                ReplaceAyats(ayats);

                // =================================================
                // 3. RESTORE POSISI AYAT
                //
                // Instance Ayat lama sudah tidak valid setelah
                // reload, sehingga dicari ulang berdasarkan
                // Surah + Nomor Ayat yang sama.
                //
                // Jika tidak ditemukan, gunakan ayat pertama Juz.
                // =================================================

                var restoredAyat =
                    previousSurah is int surah &&
                    previousAyatNumber is int number
                        ? FindAyatByPosition(
                            ayats,
                            surah,
                            number)
                        : null;

                var targetAyat =
                    restoredAyat
                    ?? ayats.FirstOrDefault();

                if (targetAyat is null)
                    return;

                SelectedAyat =
                    targetAyat;

                // SelectedSurah hanya informasi Surah
                // dari ayat aktif, bukan context utama.
                SelectedSurah =
                    Surahs.FirstOrDefault(
                        s => s.Id == targetAyat.Surah);

                Debug.WriteLine(
                    $"[FONT MODE] Ayat aktif Juz = " +
                    $"{targetAyat.Surah}:{targetAyat.AyatNumber}");
            }
            finally
            {
                _isLoadingJuz = false;
            }

            // =====================================================
            // 4. JIKA MODE HALAMAN
            //    BUKA HALAMAN SESUAI AYAT AKTIF
            // =====================================================

            if (DisplayViewMode ==
                QuranDisplayMode.Halaman)
            {
                await OpenSelectedAyatPageAsync();
            }
            else
            {
                CurrentPage = null;
            }

            // =====================================================
            // 5. UPDATE UI
            // =====================================================

            UpdateAyatPosition();

            NotifyNavigationCommands();

            Debug.WriteLine(
                "[FONT MODE] Reload Juz selesai.");

            Debug.WriteLine(
                "========================================");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                "[FONT MODE ERROR - JUZ]");

            Debug.WriteLine(
                ex.ToString());
        }
        finally
        {
            if (requestId == _loadAyatRequestId)
            {
                IsLoadingAyat = false;
            }
        }
    }

    #endregion


    #region Load Surah

    /// <summary>
    /// Memuat seluruh daftar Surah.
    /// </summary>
    public async Task LoadSurahAsync()
    {
        var list =
            await _repository
                .GetAllSurahAsync();

        Surahs.Clear();

        foreach (var surah in list)
        {
            Surahs.Add(surah);
        }
    }

    #endregion


    #region Load Ayat

    /// <summary>
    /// Memuat seluruh ayat dari Surah.
    ///
    /// Translation dimuat bersamaan
    /// kemudian digabungkan.
    /// </summary>
    public async Task LoadAyatAsync(
        int surahNumber)
    {
        var requestId =
            Interlocked.Increment(
                ref _loadAyatRequestId);

        var stopwatch =
            Stopwatch.StartNew();

        try
        {
            IsLoadingAyat = true;

            var surah =
                Surahs.FirstOrDefault(
                    x => x.Id == surahNumber);

            LoadingAyatText =
                surah is not null
                    ? $"Memuat {surah.LatinName}..."
                    : "Memuat ayat...";

            /*
             * DATABASE
             */
            var databaseTimer =
                Stopwatch.StartNew();

            var ayatTask =
                _readerContext.GetAyatAsync(
                    surahNumber);

            var translationTask =
                _translationRepository
                    .GetTranslationsBySurahAsync(
                        surahNumber);

            await Task.WhenAll(
                ayatTask,
                translationTask);

            var list =
                await ayatTask;

            var translations =
                await translationTask;

            databaseTimer.Stop();

            Debug.WriteLine(
                $"[PERFORMANCE] Database : " +
                $"{databaseTimer.ElapsedMilliseconds} ms");

            /*
             * Pastikan request masih terbaru.
             */
            if (requestId != _loadAyatRequestId)
                return;

            /*
             * MERGE TRANSLATION
             */
            MergeTranslations(
                list,
                translations);

            if (requestId != _loadAyatRequestId)
                return;

            /*
             * MASUKKAN AYAT KE UI
             */
            ReplaceAyats(list);

            /*
             * Bentuk halaman yang dimiliki Surah.
             */
            LoadPages(list);

            /*
             * Pastikan total halaman Quran
             * sudah diketahui.
             */
            if (TotalQuranPages <= 0)
            {
                await LoadTotalQuranPagesAsync();
            }

            stopwatch.Stop();

            Debug.WriteLine(
                $"[PERFORMANCE] TOTAL Surah " +
                $"{surahNumber} : " +
                $"{stopwatch.ElapsedMilliseconds} ms");

            Debug.WriteLine(
                $"[LOAD] Surah {surahNumber} " +
                $"berhasil dimuat.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"LoadAyatAsync Error - " +
                $"Surah {surahNumber}");

            Debug.WriteLine(
                ex.ToString());
        }
        finally
        {
            if (requestId == _loadAyatRequestId)
            {
                IsLoadingAyat = false;

                LoadingAyatText =
                    string.Empty;
            }
        }
    }

    /// <summary>
    /// Menggabungkan translation dengan data ayat.
    /// </summary>
    private void MergeTranslations(
        IReadOnlyList<Ayat> ayats,
        IReadOnlyList<Translation> translations)
    {
        var translationMap =
            translations.ToDictionary(
                x => x.Aya);

        foreach (var ayat in ayats)
        {
            if (!translationMap.TryGetValue(
                    ayat.AyatNumber,
                    out var translation))
            {
                continue;
            }

            ayat.Terjemahan =
                translation.Text;

            ayat.NoFn =
                translation.NoFn;

            ayat.TextFn =
                translation.TextFn
                ?? string.Empty;
        }
    }

    private async Task MergeJuzTranslationsAsync(
    IReadOnlyList<Ayat> ayats)
    {
        if (ayats.Count == 0)
            return;

        Debug.WriteLine(
            "[JUZ TRANSLATION] Mulai memuat terjemahan...");

        /*
         * Cari semua Surah yang terdapat
         * di dalam Juz.
         *
         * Contoh Juz yang hanya berisi Al-Baqarah:
         *
         * 2
         *
         * Juz yang melewati beberapa Surah:
         *
         * 2, 3, 4
         */
        var surahNumbers =
            ayats
                .Select(x => x.Surah)
                .Distinct()
                .ToList();

        Debug.WriteLine(
            $"[JUZ TRANSLATION] " +
            $"Jumlah Surah = {surahNumbers.Count}");

        /*
         * Ambil translation untuk semua Surah
         * secara bersamaan.
         */
        var translationTasks =
            surahNumbers.Select(
                surahNumber =>
                    _translationRepository
                        .GetTranslationsBySurahAsync(
                            surahNumber))
            .ToList();

        var translationResults =
            await Task.WhenAll(
                translationTasks);

        /*
         * Gabungkan semua translation
         * menjadi satu dictionary.
         *
         * KEY:
         * Surah + Nomor Ayat
         */
        var translationMap =
            new Dictionary<
                (int Surah, int Ayat),
                Translation>();

        for (int i = 0;
             i < surahNumbers.Count;
             i++)
        {
            var surahNumber =
                surahNumbers[i];

            var translations =
                translationResults[i];

            foreach (var translation
                     in translations)
            {
                translationMap[
                    (surahNumber, translation.Aya)] =
                    translation;
            }
        }

        /*
         * Pasangkan translation
         * ke masing-masing Ayat.
         */
        foreach (var ayat in ayats)
        {
            if (!translationMap.TryGetValue(
                    (ayat.Surah, ayat.AyatNumber),
                    out var translation))
            {
                continue;
            }

            ayat.Terjemahan =
                translation.Text;

            ayat.NoFn =
                translation.NoFn;

            ayat.TextFn =
                translation.TextFn
                ?? string.Empty;
        }

        Debug.WriteLine(
            $"[JUZ TRANSLATION] " +
            $"Translation berhasil digabungkan.");

        Debug.WriteLine(
            $"[JUZ TRANSLATION] " +
            $"Total Ayat = {ayats.Count}");
    }


    private void PrepareSurahHeaders(
    IReadOnlyList<Ayat> ayats)
    {
        if (ayats is null || ayats.Count == 0)
            return;

        foreach (var ayat in ayats)
        {
            var surah =
                Surahs.FirstOrDefault(
                    s => s.Id == ayat.Surah);

            if (surah is null)
                continue;

            // Hanya ayat pertama Surah
            ayat.IsSurahStart =
                ayat.AyatNumber == 1;

            // Nama Surah Arab
            ayat.SurahArabicName =
                surah.ArabicName?.Trim()
                ?? string.Empty;

            // Kategori
            ayat.SurahCategoryArabic =
                surah.Category switch
                {
                    "Madaniyah" => "مدنية",
                    "Makkiyah" => "مكية",
                    _ => surah.Category ?? string.Empty
                };

            // Jumlah ayat
            ayat.SurahTotalAyahArabic =
                ArabicNumberHelper.ToArabicNumber(
                    surah.TotalAyah);

            // At-Taubah tidak memiliki Bismillah
            ayat.ShowBismillah =
                ayat.IsSurahStart &&
                ayat.Surah != 1 &&
                ayat.Surah != 9;
        }
    }

    /// <summary>
    /// Mengganti seluruh isi ObservableCollection Ayats.
    /// </summary>
    private void ReplaceAyats(
    IReadOnlyList<Ayat> ayats)
    {
        // Siapkan informasi header Surah
        // sebelum data masuk ke UI.
        PrepareSurahHeaders(ayats);

        // Ensure collection modifications are done on UI thread.
        if (_uiDispatcher is not null)
        {
            _uiDispatcher.TryEnqueue(() =>
            {
                Ayats.Clear();

                foreach (var ayat in ayats)
                {
                    Ayats.Add(ayat);
                }

                SelectedAyat = Ayats.FirstOrDefault();
            });
        }
        else
        {
            Ayats.Clear();

            foreach (var ayat in ayats)
            {
                Ayats.Add(ayat);
            }

            SelectedAyat = Ayats.FirstOrDefault();
        }
    }

    #endregion


    #region Page / Mushaf

    /// <summary>
    /// Membentuk QuranPage dari ayat yang sudah dimuat.
    ///
    /// Digunakan ketika sebuah Surah selesai dimuat.
    /// </summary>
    private void LoadPages(
        IReadOnlyList<Ayat> ayats)
    {
        Debug.WriteLine(
            "--------------------------------");

        Debug.WriteLine(
            "[PAGE] Mulai LoadPages...");

        Debug.WriteLine(
            $"[PAGE] Input ayat : {ayats.Count}");

        if (ayats.Count == 0)
        {
            Pages =
                Array.Empty<QuranPage>();

            CurrentPage = null;

            Debug.WriteLine(
                "[PAGE] Tidak ada ayat.");

            return;
        }

        Debug.WriteLine(
            "[PAGE] Memanggil " +
            "QuranPageService.BuildPages...");

        var pages =
            _quranPageService
                .BuildPages(
                    ayats,
                    Surahs);

        Pages = pages;

        Debug.WriteLine(
            $"[PAGE] Jumlah halaman : " +
            $"{Pages.Count}");

        /*
         * Tentukan halaman berdasarkan
         * ayat yang sedang dipilih.
         */
        QuranPage? initialPage = null;

        if (SelectedAyat?.Halaman is int halaman)
        {
            initialPage =
                Pages.FirstOrDefault(
                    page =>
                        page.PageNumber == halaman);
        }

        /*
         * Jika tidak ditemukan,
         * gunakan halaman pertama.
         */
        CurrentPage =
            initialPage ??
            Pages.FirstOrDefault();

        Debug.WriteLine(
            CurrentPage is not null
                ? $"[PAGE] Current Page : " +
                  $"{CurrentPage.PageNumber}"
                : "[PAGE] Belum ada data halaman.");

        Debug.WriteLine(
            "--------------------------------");
    }


    /// <summary>
    /// Membuka halaman berdasarkan ayat
    /// yang sedang dipilih.
    ///
    /// Digunakan ketika user memilih
    /// Mode Halaman.
    /// </summary>
    private async Task OpenSelectedAyatPageAsync()
    {
        if (SelectedAyat is null)
        {
            Debug.WriteLine(
                "[PAGE] Tidak ada SelectedAyat.");

            return;
        }

        if (SelectedAyat.Halaman is not int halaman)
        {
            Debug.WriteLine(
                "[PAGE] SelectedAyat tidak memiliki " +
                "nomor halaman.");

            return;
        }

        /*
         * Jika halaman tersebut sudah ada
         * dalam Pages, gunakan langsung.
         */
        var existingPage =
            Pages.FirstOrDefault(
                page =>
                    page.PageNumber == halaman);

        if (existingPage is not null)
        {
            CurrentPage =
                existingPage;

            UpdateAyatPosition();

            return;
        }

        /*
         * Jika belum ada,
         * ambil langsung dari database.
         */
        await LoadPageAsync(halaman);
    }


    /// <summary>
    /// Memuat satu halaman Quran berdasarkan
    /// nomor halaman global.
    ///
    /// Ini merupakan pusat navigasi Mushaf.
    /// </summary>
    private async Task LoadPageAsync(
        int pageNumber)
    {
        if (pageNumber <= 0)
        {
            Debug.WriteLine(
                "[PAGE] Nomor halaman tidak valid.");

            return;
        }

        if (TotalQuranPages > 0 &&
            pageNumber > TotalQuranPages)
        {
            Debug.WriteLine(
                $"[PAGE] Halaman {pageNumber} " +
                $"melebihi total " +
                $"{TotalQuranPages}.");

            return;
        }

        try
        {
            Debug.WriteLine(
                "--------------------------------");

            Debug.WriteLine(
                $"[PAGE NAVIGATION] " +
                $"Load halaman {pageNumber}");

            /*
             * Ambil ayat berdasarkan nomor halaman.
             *
             * Method ini harus tersedia di ReaderContext.
             */
            var ayats =
                await _readerContext
                    .GetAyatByPageAsync(
                        pageNumber);

            if (ayats is null ||
                ayats.Count == 0)
            {
                Debug.WriteLine(
                    $"[PAGE NAVIGATION] " +
                    $"Halaman {pageNumber} " +
                    $"tidak memiliki data.");

                return;
            }

            Debug.WriteLine(
                $"[PAGE NAVIGATION] " +
                $"Jumlah ayat : {ayats.Count}");

            /*
             * Bangun QuranPage.
             */
            var pages =
                _quranPageService
                    .BuildPages(
                        ayats,
                        Surahs);

            var page =
                pages.FirstOrDefault(
                    x =>
                        x.PageNumber ==
                        pageNumber);

            if (page is null)
            {
                Debug.WriteLine(
                    $"[PAGE NAVIGATION] " +
                    $"QuranPage {pageNumber} " +
                    $"gagal dibentuk.");

                return;
            }

            /*
             * Simpan hanya halaman aktif
             * sebagai context halaman.
             */
            Pages =
                new[]
                {
                    page
                };

            /*
             * Update halaman aktif.
             */
            CurrentPage =
                page;

            /*
             * Saat Mode Halaman,
             * SelectedAyat mengikuti ayat pertama.
             */
            var firstAyat =
                ayats.FirstOrDefault();

            if (firstAyat is not null)
            {
                SelectedAyat =
                    firstAyat;
            }

            UpdateAyatPosition();

            NotifyNavigationCommands();

            Debug.WriteLine(
                $"[PAGE NAVIGATION] " +
                $"Halaman {pageNumber} berhasil dimuat.");

            Debug.WriteLine(
                $"[PAGE NAVIGATION] " +
                $"Surah pertama : " +
                $"{firstAyat?.Surah}");

            Debug.WriteLine(
                $"[PAGE NAVIGATION] " +
                $"Ayat pertama : " +
                $"{firstAyat?.AyatNumber}");

            Debug.WriteLine(
                "--------------------------------");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"[PAGE NAVIGATION ERROR] " +
                $"Halaman {pageNumber}");

            Debug.WriteLine(
                ex.ToString());
        }
    }


    /// <summary>
    /// Mengambil jumlah seluruh halaman Quran.
    /// </summary>
    private async Task LoadTotalQuranPagesAsync()
    {
        try
        {
            TotalQuranPages =
                await _readerContext
                    .GetTotalPageAsync();

            Debug.WriteLine(
                $"[PAGE] Total halaman Quran : " +
                $"{TotalQuranPages}");

            OnPropertyChanged(
                nameof(HasPreviousPage));

            OnPropertyChanged(
                nameof(HasNextPage));

            UpdateAyatPosition();

            NotifyNavigationCommands();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                "[PAGE ERROR] " +
                "Gagal mengambil total halaman.");

            Debug.WriteLine(
                ex.ToString());
        }
    }

    #endregion


    #region Navigation

    /// <summary>
    /// Menentukan apakah tombol Next dapat digunakan.
    ///
    /// Mode Ayat:
    /// Next ayat.
    ///
    /// Mode Halaman:
    /// Next halaman.
    /// </summary>
    private bool CanNextAyat()
    {
        if (DisplayViewMode ==
            QuranDisplayMode.Halaman)
        {
            return HasNextPage;
        }

        if (Ayats.Count == 0)
            return false;

        if (SelectedAyat is null)
            return Ayats.Count > 1;

        var currentIndex =
            Ayats.IndexOf(
                SelectedAyat);

        return currentIndex >= 0 &&
               currentIndex < Ayats.Count - 1;
    }


    /// <summary>
    /// Menentukan apakah tombol Previous dapat digunakan.
    /// </summary>
    private bool CanPreviousAyat()
    {
        if (DisplayViewMode ==
            QuranDisplayMode.Halaman)
        {
            return HasPreviousPage;
        }

        if (Ayats.Count == 0)
            return false;

        if (SelectedAyat is null)
            return false;

        var currentIndex =
            Ayats.IndexOf(
                SelectedAyat);

        return currentIndex > 0;
    }


    /// <summary>
    /// Next:
    ///
    /// Mode Ayat    → ayat berikutnya.
    /// Mode Halaman → halaman berikutnya.
    /// </summary>
    [RelayCommand(
        CanExecute = nameof(CanNextAyat))]
    private async Task NextAyatAsync()
    {
        if (DisplayViewMode ==
            QuranDisplayMode.Halaman)
        {
            await NextPageAsync();
            return;
        }

        NextAyat();
    }


    /// <summary>
    /// Previous:
    ///
    /// Mode Ayat    → ayat sebelumnya.
    /// Mode Halaman → halaman sebelumnya.
    /// </summary>
    [RelayCommand(
        CanExecute = nameof(CanPreviousAyat))]
    private async Task PreviousAyatAsync()
    {
        if (DisplayViewMode ==
            QuranDisplayMode.Halaman)
        {
            await PreviousPageAsync();
            return;
        }

        PreviousAyat();
    }


    /// <summary>
    /// Navigasi ayat berikutnya.
    /// </summary>
    private void NextAyat()
    {
        if (Ayats.Count == 0)
            return;

        if (SelectedAyat is null)
        {
            SelectedAyat =
                Ayats.FirstOrDefault();

            return;
        }

        var currentIndex =
            Ayats.IndexOf(
                SelectedAyat);

        if (currentIndex < 0)
        {
            SelectedAyat =
                Ayats.FirstOrDefault();

            return;
        }

        if (currentIndex >=
            Ayats.Count - 1)
        {
            return;
        }

        SelectedAyat =
            Ayats[currentIndex + 1];
    }


    /// <summary>
    /// Navigasi ayat sebelumnya.
    /// </summary>
    private void PreviousAyat()
    {
        if (Ayats.Count == 0)
            return;

        if (SelectedAyat is null)
        {
            SelectedAyat =
                Ayats.FirstOrDefault();

            return;
        }

        var currentIndex =
            Ayats.IndexOf(
                SelectedAyat);

        if (currentIndex <= 0)
            return;

        SelectedAyat =
            Ayats[currentIndex - 1];
    }


    /// <summary>
    /// Navigasi halaman berikutnya.
    ///
    /// Contoh:
    /// Page 1 → Page 2
    /// Page 2 → Page 3
    /// </summary>
    private async Task NextPageAsync()
    {
        if (CurrentPage is null)
            return;

        var nextPageNumber =
            CurrentPage.PageNumber + 1;

        if (nextPageNumber >
            TotalQuranPages)
        {
            Debug.WriteLine(
                "[PAGE NAVIGATION] " +
                "Sudah berada di halaman terakhir.");

            return;
        }

        Debug.WriteLine(
            $"[PAGE NAVIGATION] " +
            $"Next: {CurrentPage.PageNumber} " +
            $"→ {nextPageNumber}");

        await LoadPageAsync(
            nextPageNumber);
    }


    /// <summary>
    /// Navigasi halaman sebelumnya.
    ///
    /// Contoh:
    /// Page 3 → Page 2
    /// Page 2 → Page 1
    /// </summary>
    private async Task PreviousPageAsync()
    {
        if (CurrentPage is null)
            return;

        var previousPageNumber =
            CurrentPage.PageNumber - 1;

        if (previousPageNumber < 1)
        {
            Debug.WriteLine(
                "[PAGE NAVIGATION] " +
                "Sudah berada di halaman pertama.");

            return;
        }

        Debug.WriteLine(
            $"[PAGE NAVIGATION] " +
            $"Previous: {CurrentPage.PageNumber} " +
            $"→ {previousPageNumber}");

        await LoadPageAsync(
            previousPageNumber);
    }


    /// <summary>
    /// Refresh status tombol Previous / Next.
    /// </summary>
    private void NotifyNavigationCommands()
    {
        try
        {
            NextAyatCommand
                .NotifyCanExecuteChanged();

            PreviousAyatCommand
                .NotifyCanExecuteChanged();
        }
        catch
        {
            /*
             * Command mungkin belum dibuat
             * oleh source generator pada saat
             * constructor / initialization tertentu.
             */
        }
    }

    #endregion


    #region Selection Synchronization

    /// <summary>
    /// Dipanggil ketika SelectedSurah berubah.
    /// </summary>
    partial void OnSelectedSurahChanged(
    Surah? value)
    {
        if (value is null)
        {
            Ayats.Clear();

            SelectedAyat = null;

            Pages =
                Array.Empty<QuranPage>();

            CurrentPage = null;

            UpdateAyatPosition();

            NotifyNavigationCommands();

            return;
        }
        // When SelectedSurah changes (user picks a Surah), always load the full
        // Surah ayats into SurahAyats for navigation ComboBox. The Reader's Ayats
        // (which may represent a Juz context) are left unchanged unless we are
        // intentionally loading the Surah via HandleSelectedSurahChangedAsync.
        // This ensures the Ayah ComboBox always lists the entire Surah (1..N).
        // Ensure Surah navigation list is populated for the toolbar.
        // Awaiting is not necessary here; LoadSurahAyatsAsync will update SurahAyats
        // and the ComboBox SelectedItem will be resolved once population completes.
        _ = LoadSurahAyatsAsync(value.Id);

        Debug.WriteLine(
            $"[SURAH] Selected Surah = " +
            $"{value.Id} - {value.LatinName}");

        // =====================================================
        // PENTING:
        // Jika perubahan Surah terjadi karena LoadJuzAsync(),
        // JANGAN load ulang seluruh Surah.
        // =====================================================

        if (_isLoadingJuz)
        {
            // SelectedSurah was changed as part of a Juz load. Do not trigger a
            // full Surah load in this path since the Reader's Ayats already reflect
            // the Juz context.
            return;
        }

        // =====================================================
        // Jika bukan proses Juz,
        // berarti user memang memilih Surah secara langsung.
        //
        // Context Juz yang mungkin masih aktif dari
        // pemilihan Juz sebelumnya harus dibersihkan,
        // agar perubahan Reader Mode berikutnya tidak
        // salah menganggap context masih Juz.
        // =====================================================

        _activeJuzNumber = null;

        _ = HandleSelectedSurahChangedAsync(
            value);
    }


    /// <summary>
    /// Handler async untuk perubahan Surah.
    /// </summary>
    private async Task HandleSelectedSurahChangedAsync(
    Surah? value)
    {
        try
        {
            if (value is null)
                return;

            Debug.WriteLine(
                $"[SURAH] Memuat seluruh Surah " +
                $"{value.Id} - {value.LatinName}");

            await LoadAyatAsync(
                value.Id);

            // =====================================================
            // Jika Mode Halaman,
            // buka halaman sesuai ayat aktif.
            // =====================================================

            if (DisplayViewMode ==
                QuranDisplayMode.Halaman)
            {
                await OpenSelectedAyatPageAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                "[SURAH ERROR]");

            Debug.WriteLine(
                ex.ToString());
        }
    }


    /// <summary>
    /// Dipanggil ketika SelectedAyat berubah.
    /// </summary>
    partial void OnSelectedAyatChanged(
        Ayat? value)
    {
        UpdateAyatPosition();

        // Keep toolbar numeric selection in sync with the actual selected Ayat.
        // Use AyatNumber as stable identifier so the toolbar ComboBox which
        // binds to SelectedAyatNumber (SelectedValue) can resolve selection
        // even when the instance comes from a different collection.
        try
        {
            if (value is not null)
            {
                SelectedAyatNumber = value.AyatNumber;
            }
            else
            {
                SelectedAyatNumber = null;
            }
        }
        catch
        {
            // suppress errors during synchronization
        }

        // =====================================================
        // Synchronize SelectedJuz for toolbar display only.
        // Do NOT call LoadJuzAsync here. Use a suppression flag
        // so that the SelectedJuz setter does not trigger a
        // reload via OnSelectedJuzChanged.
        // =====================================================
        try
        {
            if (value is not null)
            {
                var juzNumber = value.Juz;

                var juz = JuzList.FirstOrDefault(x => x.JuzNumber == juzNumber);

                if (juz is not null && SelectedJuz?.JuzNumber != juzNumber)
                {
                    // Synchronize SelectedJuz for toolbar display only.
                    _suppressSelectedJuzChanged = true;
                    SelectedJuz = juz;
                    _suppressSelectedJuzChanged = false;
                }

                // Auto-navigation: if the selected ayat is not present in the current
                // reader context (Ayats), we must load the juz that contains it and
                // then navigate to the ayat. If it is present, instruct ReaderView
                // to scroll to it (ReaderView listens to SelectedAyat property).

                // If ayat is outside current Ayats, schedule a Juz load and remember
                // the requested navigation so that after LoadJuzAsync finishes the
                // reader can scroll to the correct ayat.
                if (_activeJuzNumber is not null && _activeJuzNumber != value.Juz)
                {
                    // Target ayat is in another juz.
                    _pendingNavigation = (value.Surah, value.AyatNumber, value.Juz);

                    // Trigger loading of the target juz. LoadJuzAsync is guarded and
                    // idempotent when the requested juz is already active.
                    _ = LoadJuzAsync(value.Juz);
                }
                else
                {
                    // If ayat is inside current Ayats or there is no active juz,
                    // do nothing here; ReaderView will respond to SelectedAyat change
                    // and scroll via its Own property change handler.
                }
            }
        }
        catch
        {
            // suppress exceptions from synchronization logic
        }

        /*
         * Jika user sedang berada pada Mode Halaman,
         * perubahan ayat harus membuka halaman
         * tempat ayat tersebut berada.
         */
        if (DisplayViewMode ==
            QuranDisplayMode.Halaman)
        {
            _ = OpenSelectedAyatPageAsync();
        }

        NotifyNavigationCommands();
    }

    /// <summary>
    /// Called when SelectedAyatNumber (the numeric selection in the toolbar)
    /// changes. Resolve the numeric selection to an Ayat instance. Prefer
    /// the instance that exists in the current Reader collection (Ayats)
    /// so ReaderView can perform object-identity based scrolling; if not
    /// present, fall back to the Surah navigation instance which will
    /// trigger navigation to the proper Juz if required.
    /// </summary>
    partial void OnSelectedAyatNumberChanged(int? value)
    {
        try
        {
            if (value is null)
                return;

            if (SelectedSurah is null)
                return;

            var surahNumber = SelectedSurah.Id;

            // Prefer the Ayat instance that belongs to current reader context.
            var inReader = FindAyatByPosition(Ayats, surahNumber, value.Value);
            if (inReader is not null)
            {
                if (!ReferenceEquals(SelectedAyat, inReader))
                    SelectedAyat = inReader;

                return;
            }

            // Fall back to the Surah navigation list instance (if present).
            var inSurahList = SurahAyats.FirstOrDefault(x => x.Surah == surahNumber && x.AyatNumber == value.Value);
            if (inSurahList is not null)
            {
                if (!ReferenceEquals(SelectedAyat, inSurahList))
                    SelectedAyat = inSurahList;
            }
        }
        catch
        {
            // suppress sync errors
        }
    }


    /// <summary>
    /// Memilih ayat berdasarkan nomor ayat.
    /// </summary>
    public void SelectAyat(
        int ayatNumber)
    {
        var ayat =
            Ayats.FirstOrDefault(
                x => x.AyatNumber == ayatNumber);

        if (ayat is null)
            return;

        SelectedAyat =
            ayat;
    }


    /// <summary>
    /// Sinkronisasi halaman berdasarkan ayat aktif.
    ///
    /// Digunakan ketika:
    /// Mode Ayat → Mode Halaman.
    /// </summary>
    private async Task SyncPageWithSelectedAyatAsync()
    {
        if (SelectedAyat is null)
            return;

        if (SelectedAyat.Halaman is not int halaman)
        {
            Debug.WriteLine(
                "[PAGE] SelectedAyat tidak " +
                "memiliki nomor halaman.");

            return;
        }

        await LoadPageAsync(
            halaman);
    }


    /// <summary>
    /// Sinkronisasi ayat berdasarkan halaman aktif.
    ///
    /// Digunakan ketika:
    /// Mode Halaman → Mode Ayat.
    /// </summary>
    private void SyncAyatWithCurrentPage()
    {
        Debug.WriteLine("[PAGE SYNC DEBUG]");
        Debug.WriteLine($"CurrentPage = {(CurrentPage is null ? "NULL" : "NOT NULL")}");
        if (CurrentPage is not null)
        {
            Debug.WriteLine($"CurrentPage.Ayats.Count = { (CurrentPage.Ayats is null ? 0 : CurrentPage.Ayats.Count) }");
        }
        Debug.WriteLine($"Ayats.Count = {Ayats.Count}");
        Debug.WriteLine($"DisplayViewMode = {DisplayViewMode}");

        if (CurrentPage?.Ayats is not
            { Count: > 0 })
        {
            return;
        }

        var firstAyat =
            CurrentPage.Ayats
                .FirstOrDefault();

        if (firstAyat is null)
            return;

        SelectedAyat =
            firstAyat;

        Debug.WriteLine(
            $"[PAGE] Sync CurrentPage → " +
            $"Ayat {firstAyat.AyatNumber}");
    }

    #endregion


    #region Position

    /// <summary>
    /// Memperbarui teks posisi pada navigation bar.
    ///
    /// Mode Ayat:
    /// Ayat X / Y
    ///
    /// Mode Halaman:
    /// Halaman X / Y
    /// </summary>
    private void UpdateAyatPosition()
    {
        if (DisplayViewMode ==
            QuranDisplayMode.Halaman)
        {
            if (CurrentPage is null)
            {
                CurrentAyatPosition =
                    string.Empty;

                return;
            }

            if (TotalQuranPages > 0)
            {
                CurrentAyatPosition =
                    $"Halaman " +
                    $"{CurrentPage.PageNumber} / " +
                    $"{TotalQuranPages}";
            }
            else
            {
                CurrentAyatPosition =
                    $"Halaman " +
                    $"{CurrentPage.PageNumber}";
            }

            return;
        }

        if (SelectedAyat is null)
        {
            CurrentAyatPosition =
                string.Empty;

            return;
        }

        var index =
            Ayats.IndexOf(
                SelectedAyat);

        if (index < 0)
        {
            CurrentAyatPosition =
                string.Empty;

            return;
        }

        CurrentAyatPosition =
            $"Ayat {index + 1} / " +
            $"{Ayats.Count}";
    }

    #endregion


    #region Logging

    /// <summary>
    /// Mencatat perubahan mode reader.
    /// </summary>
    private void LogDisplayMode(
        ReaderMode oldMode,
        ReaderMode newMode)
    {
        _logService.Separator();

        _logService.Write(
            LogMessages.DisplayModeChanged);

        _logService.Separator();

        _logService.Write(
            $"Mode Lama : {oldMode}");

        _logService.Write(
            $"Mode Baru : {newMode}");

        _logService.Separator();
    }

    #endregion


    public async Task LoadJuzAsync(int juzNumber)
    {
        var requestId =
            Interlocked.Increment(
                ref _loadAyatRequestId);

        try
        {
            // Begin loading Juz

            IsLoadingAyat = true;

            _isLoadingJuz = true;

            _activeJuzNumber = juzNumber;

            SelectedJuz =
                new JuzInfo
                {
                    JuzNumber = juzNumber
                };

            // 1. Ambil seluruh ayat dalam Juz
            var ayats =
                await _readerContext
                    .GetAyatByJuzAsync(juzNumber);

            /*
             * Pastikan request masih terbaru.
             * Mencegah request Juz lama menimpa
             * request terbaru (mis. user cepat berpindah Juz).
             */
            if (requestId != _loadAyatRequestId)
                return;

            if (ayats == null || ayats.Count == 0)
            {
                // No data for this juz in current reader mode: clear selection.
                SelectedSurah = null;
                NotifyNavigationCommands();
                return;
            }

            // Internal: number of ayats loaded for juz.
            // Diagnostic debug logs removed.

            //Load translation
            await MergeJuzTranslationsAsync(ayats);

            if (requestId != _loadAyatRequestId)
            {
                return;
            }

            // Translation test logs suppressed.

            // 2. Simpan seluruh ayat ke collection reader
            ReplaceAyats(ayats);

            // Diagnostic dump suppressed.

            // 3. Ambil ayat pertama Juz
            var firstAyat =
                ayats.FirstOrDefault();

            if (firstAyat == null)
                return;

            // 4. Jadikan ayat pertama sebagai posisi aktif
            // Load Surah navigation list on background, then marshal UI updates
            // to the UI dispatcher synchronously (no async lambda inside TryEnqueue).
            var loadSurahTask = LoadSurahAyatsAsync(firstAyat.Surah);

            // Wait for Surah data to be loaded (background thread).
            await loadSurahTask.ConfigureAwait(false);

            // Now apply UI-bound state on the UI dispatcher.
            if (_uiDispatcher is not null)
            {
                var tcsSet = new TaskCompletionSource<int>();

                _uiDispatcher.TryEnqueue(() =>
                {
                    try
                    {
                        // Set active surah for UI.
                        SelectedSurah = Surahs.FirstOrDefault(s => s.Id == firstAyat.Surah);

                        // Set selected ayat (use the instance that will be present
                        // in the reader collection after ReplaceAyats).
                        SelectedAyat = firstAyat;

                        tcsSet.SetResult(0);
                    }
                    catch (Exception ex)
                    {
                        tcsSet.SetException(ex);
                    }
                });

                await tcsSet.Task.ConfigureAwait(false);
            }
            else
            {
                // Fallback: apply directly (rare).
                SelectedSurah = Surahs.FirstOrDefault(s => s.Id == firstAyat.Surah);
                SelectedAyat = firstAyat;
            }

            // If there is a pending navigation (user selected an Ayat from toolbar
            // that required switching juz), and it matches this juz, clear it and
            // ensure SelectedAyat is the pending target so ReaderView will scroll.
            if (_pendingNavigation is (int s, int a, int j) pending && pending.Juz == juzNumber)
            {
                // Try to find the exact Ayat instance within the newly loaded Ayats
                // and set it as SelectedAyat so that ReaderView will scroll to it.
                var target = Ayats.FirstOrDefault(x => x.Surah == s && x.AyatNumber == a);
                if (target is not null)
                {
                    SelectedAyat = target;
                }

                _pendingNavigation = null;
            }

            //// 6. Bangun data halaman dari seluruh ayat Juz
            //LoadPages(ayats);

            //// 7. Jika mode Halaman,
            ////    cari halaman tempat ayat pertama Juz berada.
            //if (DisplayViewMode == QuranDisplayMode.Halaman)
            //{
            //    var firstPage =
            //        Pages.FirstOrDefault(
            //            p => p.PageNumber == firstAyat.Halaman);

            //    if (firstPage != null)
            //    {
            //        CurrentPage = firstPage;

            //        Debug.WriteLine(
            //            $"[JUZ LOAD] CurrentPage = " +
            //            $"{CurrentPage.PageNumber}");
            //    }
            //}

            // =====================================================
            // 6. MODE AYAT
            //
            // Juz dianggap sebagai satu context reader.
            // Seluruh ayat Juz sudah berada di Ayats.
            //
            // Jangan panggil LoadPages(ayats) di sini.
            // Karena BuildPages() akan menganggap
            // 286 ayat tersebut sebagai context halaman lokal.
            // =====================================================

            if (DisplayViewMode ==
                QuranDisplayMode.Ayat)
            {
                CurrentPage = null;

                UpdateAyatPosition();

                // Juz displayed in Ayat mode.
            }

            // =====================================================
            // 7. MODE HALAMAN
            //
            // Ambil halaman berdasarkan nomor halaman
            // dari ayat pertama Juz.
            // =====================================================

            if (DisplayViewMode ==
                QuranDisplayMode.Halaman)
            {
                if (firstAyat.Halaman is int halaman)
                {
                    Debug.WriteLine(
                        $"[JUZ LOAD] Membuka halaman " +
                        $"{halaman} berdasarkan ayat pertama Juz.");

                    await LoadPageAsync(
                        halaman);
                }
                else
                {
                    Debug.WriteLine(
                        "[JUZ LOAD] Ayat pertama Juz " +
                        "tidak memiliki nomor halaman.");
                }
            }

            Debug.WriteLine(
                $"[JUZ LOAD] Juz {juzNumber} selesai.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"[JUZ LOAD ERROR] Juz={juzNumber}");

            Debug.WriteLine(ex);
        }
        finally
        {
            _isLoadingJuz = false;

            if (requestId == _loadAyatRequestId)
            {
                // Ensure IsLoadingAyat is cleared on UI thread.
                if (_uiDispatcher is not null)
                {
                    _uiDispatcher.TryEnqueue(() => IsLoadingAyat = false);
                }
                else
                {
                    IsLoadingAyat = false;
                }
            }
        }
    }
}