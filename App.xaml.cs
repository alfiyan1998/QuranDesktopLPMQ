// csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using QuranKemenag.Helpers;
using QuranKemenag.Repositories;
using QuranKemenag.Services;
using QuranKemenag.Services.Copy;
using QuranKemenag.Services.Font;
using QuranKemenag.Services.Logging;
using QuranKemenag.Services.Page;
using QuranKemenag.Services.Text;
using QuranKemenag.ViewModels;
using QuranKemenag.ViewModels.Page;
using System;
using System.Diagnostics;


namespace QuranKemenag;

/// <summary>
/// Entry point aplikasi. Mengonfigurasi Dependency Injection sebelum membuat MainWindow.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Service provider aplikasi yang dapat di-resolve dari kode lain.
    /// Di-set saat startup dan bertahan sepanjang lifecycle aplikasi.
    /// </summary>
    public static IServiceProvider? Services { get; private set; }

    private Window? _mainWindow;

    public App()
    {
        InitializeComponent();
        ConfigureServices();
    }

    private void ConfigureServices()
    {
        var services = new ServiceCollection();

        // DatabaseFactory membutuhkan path base aplikasi untuk menemukan file DB.
        services.AddSingleton(sp => new DatabaseFactory(AppContext.BaseDirectory));

        // DatabaseService dibuat menggunakan DatabaseFactory.
        services.AddSingleton<DatabaseService>(sp =>
        {
            var factory = sp.GetRequiredService<DatabaseFactory>();
            return new DatabaseService(factory);
        });

        services.AddSingleton<IFontManager, FontManager>();

        services.AddSingleton<ISymbolTextProcessor, SymbolTextProcessor>();

        // Repository
        services.AddSingleton<IQuranRepository, UnicodeRepository>();
        services.AddSingleton<ISymbolRepository, SymbolRepository>();
        services.AddSingleton<ITranslationRepository, TranslationRepository>();

        // Reader Service
        services.AddSingleton<ComputerFontReaderService>();
        services.AddSingleton<HandwrittenManuscriptReaderService>();
        //copy service
        services.AddSingleton<QuranCopyService>();
        services.AddTransient<QuranCopyService>();

        services.AddSingleton<ReaderServiceFactory>();
        services.AddSingleton<ReaderContext>();
        services.AddSingleton<ILogService, DebugLogService>();
        services.AddSingleton<IQuranPageService,QuranPageService>();
        services.AddSingleton<JuzRepository>();
       

        // ================================================================
        // MUSHAF PER HALAMAN (baru)
        // ================================================================

        // LayoutRepository butuh path fisik mushaf_layout.db, didapat dari DatabaseFactory.
        services.AddSingleton(sp =>
        {
            var factory = sp.GetRequiredService<DatabaseFactory>();
            var path = factory.GetDatabasePath(DatabaseType.MushafLayout);
            return new LayoutRepository(path);
        });

        services.AddSingleton<MushafPageService>();



        // ViewModel
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MushafPageViewModel>();

        // Build the provider and expose it application-wide
        Services = services.BuildServiceProvider();

        Debug.WriteLine("DI container configured. Services ready.");
       
    }

    /// <summary>
    /// Developer helper untuk memverifikasi bahwa MushafPageViewModel (via DI)
    /// berhasil memuat halaman 3 dengan 15 baris dari mushaf_layout.db.
    /// </summary>
    private static async Task TestMushafPageViewModelAsync()
    {
        try
        {
            Debug.WriteLine("--------------------------------");
            Debug.WriteLine("[MUSHAF DI TEST] Mulai...");

            if (Services is null)
            {
                Debug.WriteLine("[MUSHAF DI TEST] DI container tidak tersedia.");
                return;
            }

            var viewModel = Services.GetRequiredService<MushafPageViewModel>();

            await viewModel.LoadPageAsync(3);

            Debug.WriteLine($"[MUSHAF DI TEST] Page = {viewModel.PageNumber}");
            Debug.WriteLine($"[MUSHAF DI TEST] Jumlah baris = {viewModel.Lines.Count}");

            foreach (var line in viewModel.Lines)
            {
                Debug.WriteLine(
                    $"[MUSHAF DI TEST] LINE {line.LineNumber} | WORDS {line.Words.Count}");

                foreach (var word in line.Words)
                {
                    Debug.WriteLine(
                        $"    [{word.WordOrder}] Layout='{word.WordText}' | Unicode='{word.UnicodeText}'");
                }
            }

            Debug.WriteLine("[MUSHAF DI TEST] Selesai.");
            Debug.WriteLine("--------------------------------");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[MUSHAF DI TEST ERROR]");
            Debug.WriteLine(ex.ToString());
        }
    }

    private static async Task TestMushafHandwrittenAsync()
    {
        try
        {
            Debug.WriteLine("--------------------------------");
            Debug.WriteLine("[MUSHAF HANDWRITTEN TEST] Mulai...");

            if (Services is null)
            {
                Debug.WriteLine("[MUSHAF HANDWRITTEN TEST] DI container tidak tersedia.");
                return;
            }

            var viewModel = Services.GetRequiredService<MushafPageViewModel>();

            var handwrittenMode = viewModel.DisplayModes
                .FirstOrDefault(m => m.Mode == QuranKemenag.Models.DisplayMode.HandwrittenManuscript);

            viewModel.SelectedDisplayModeItem = handwrittenMode;

            await viewModel.LoadPageAsync(3);

            Debug.WriteLine($"[MUSHAF HANDWRITTEN TEST] Page = {viewModel.PageNumber}");
            Debug.WriteLine($"[MUSHAF HANDWRITTEN TEST] ErrorMessage = '{viewModel.ErrorMessage}'");
            Debug.WriteLine($"[MUSHAF HANDWRITTEN TEST] Jumlah baris = {viewModel.Lines.Count}");

            foreach (var line in viewModel.Lines)
            {
                Debug.WriteLine($"[MUSHAF HANDWRITTEN TEST] LINE {line.LineNumber}");

                foreach (var word in line.Words)
                {
                    Debug.WriteLine(
                        $"    [{word.WordOrder}] {word.Location} | " +
                        $"DisplayText='{word.DisplayText}' | KodeFont={word.KodeFont}");
                }
            }

            Debug.WriteLine("[MUSHAF HANDWRITTEN TEST] Selesai.");
            Debug.WriteLine("--------------------------------");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[MUSHAF HANDWRITTEN TEST ERROR]");
            Debug.WriteLine(ex.ToString());
        }
    }


    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        #if DEBUG
            QuranKemenag.Services.Page.QuranPageServiceDebugTest.Run();
        #endif
        if (Services is null)
        {
            throw new InvalidOperationException("DI container belum dikonfigurasi. ConfigureServices harus dipanggil sebelum membuat window.");
        }

        // Kick off a fire-and-forget integration test for the symbol repository.
        // This runs in background and writes output to the Debug Output window.
        // It does not block or change UI startup.
        _ = TestSymbolRepositoryAsync();
        _ = TestMushafPageViewModelAsync(); // baru

        _mainWindow = new MainWindow();
        _mainWindow.Activate();
    }



    /// <summary>
    /// Temporary developer helper that tests <see cref="ISymbolRepository"/> by
    /// requesting ayat lists for several surah and writing results to Debug output.
    /// This method is intended for manual verification during development only.
    /// </summary>
    /// <returns>A task that completes when the test has finished writing output.</returns>
    private static async Task TestSymbolRepositoryAsync()
    {
        // Use Debug output so the information appears in Visual Studio Output window.
        try
        {
            Debug.WriteLine("--------------------------------");
            Debug.WriteLine("Starting SymbolRepository test...");

            if (Services is null)
            {
                Debug.WriteLine("DI container not available. Skipping symbol repository test.");
                return;
            }

            var repo = Services.GetRequiredService<ISymbolRepository>();

            // List of surah numbers to test
            int[] testSurahs = new[] { 1, 2, 114 };

            foreach (var surah in testSurahs)
            {
                try
                {
                    Debug.WriteLine("--------------------------------");
                    Debug.WriteLine($"SURAH {surah}");

                    var list = await repo.GetAyatBySurahAsync(surah).ConfigureAwait(false);

                    if (list is null || list.Count == 0)
                    {
                        Debug.WriteLine("Tidak ada data.");
                        continue;
                    }

                    Debug.WriteLine($"Jumlah Ayat : {list.Count}");

                    // Print up to first 300 lines to avoid flooding output, but typically
                    // surah 2 has 286 ayat so it's safe to print all.
                    int max = Math.Min(list.Count, 1000);
                    for (int i = 0; i < max; i++)
                    {
                        var item = list[i];
                        Debug.WriteLine($"{item.AyatNumber} | kode_font={item.KodeFont}");
                    }

                    // Additional diagnostic: if surah == 1, analyze Ayat 1 string in detail
                    if (surah == 1)
                    {
                        try
                        {
                            var ay1 = list.FirstOrDefault(x => x.AyatNumber == 1);
                            if (ay1 is not null)
                            {
                                Debug.WriteLine("--- Detailed string analysis for Surah 1 Ayat 1 ---");
                                var text = ay1.Teks ?? string.Empty;

                                // 1. Isi string asli
                                Debug.WriteLine("Original string:");
                                Debug.WriteLine(text);

                                // 2. Panjang string (char units)
                                Debug.WriteLine($"String.Length (char units): {text.Length}");

                                // 3. Jumlah karakter (runes / code points)
                                int runeCount = text.EnumerateRunes().Count();
                                Debug.WriteLine($"Rune count (code points): {runeCount}");

                                // 4. Each character (index by rune order)
                                Debug.WriteLine("Index | Character | Unicode (U+HEX) | Category");
                                int idx = 0;
                                foreach (var rune in text.EnumerateRunes())
                                {
                                    string charDisplay = rune.ToString();
                                    string hex = rune.Value.ToString("X4");
                                    var category = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(rune.ToString(), 0);
                                    Debug.WriteLine($"{idx,3} | {charDisplay} | U+{hex} | {category}");
                                    idx++;
                                }

                                // 5. Tampilkan semua karakter kontrol khusus jika ada (list of known bidi/controls)
                                var controlCodes = new (string name, int code)[]
                                {
                                    ("LRM", 0x200E),
                                    ("RLM", 0x200F),
                                    ("LRE", 0x202A),
                                    ("RLE", 0x202B),
                                    ("FSI", 0x2068),
                                    ("LRI", 0x2066),
                                    ("RLI", 0x2067),
                                    ("PDI", 0x2069),
                                    ("ZWJ", 0x200D),
                                    ("ZWNJ", 0x200C),
                                    ("NBSP", 0x00A0)
                                };

                                foreach (var (name, code) in controlCodes)
                                {
                                    int occurrences = text.EnumerateRunes().Count(r => r.Value == code);
                                    if (occurrences > 0)
                                    {
                                        Debug.WriteLine($"Control {name} (U+{code:X4}) occurrences: {occurrences}");
                                    }
                                }

                                // 6. Tampilkan semua karakter yang BUKAN spasi biasa (distinct)
                                var nonSpaceRunes = text.EnumerateRunes().Where(r => r.Value != 0x0020).Select(r => r.Value).Distinct();
                                Debug.WriteLine("Distinct non-space code points present:");
                                foreach (var cp in nonSpaceRunes)
                                {
                                    Debug.WriteLine($"U+{cp:X4}");
                                }

                                // 7. Hitung jumlah spesifik
                                int countSpace = text.Count(c => c == ' ');
                                int countNbsp = text.EnumerateRunes().Count(r => r.Value == 0x00A0);
                                int countRlm = text.EnumerateRunes().Count(r => r.Value == 0x200F);
                                int countLrm = text.EnumerateRunes().Count(r => r.Value == 0x200E);
                                int countZwJ = text.EnumerateRunes().Count(r => r.Value == 0x200D);
                                int countZwnj = text.EnumerateRunes().Count(r => r.Value == 0x200C);

                                Debug.WriteLine($"Counts => Space(U+0020): {countSpace}, NBSP(U+00A0): {countNbsp}, RLM(U+200F): {countRlm}, LRM(U+200E): {countLrm}, ZWJ(U+200D): {countZwJ}, ZWNJ(U+200C): {countZwnj}");

                                // 8. EnumerateRunes output (sequence of U+ codes)
                                Debug.WriteLine("EnumerateRunes sequence (U+HEX):");
                                Debug.WriteLine(string.Join(' ', text.EnumerateRunes().Select(r => "U+" + r.Value.ToString("X4"))));

                                Debug.WriteLine("--- End detailed analysis ---");
                            }
                            else
                            {
                                Debug.WriteLine("Ayat 1 not found in surah 1 list.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Error during detailed analysis: " + ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Write exception details but continue with next surah
                    Debug.WriteLine(ex.ToString());
                }
            }

            Debug.WriteLine("SymbolRepository test finished.");
            Debug.WriteLine("--------------------------------");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }
}