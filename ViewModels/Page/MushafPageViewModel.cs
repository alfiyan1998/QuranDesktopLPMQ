using CommunityToolkit.Mvvm.ComponentModel;
using QuranKemenag.Helpers;
using QuranKemenag.Models;
using QuranKemenag.Services.Page;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace QuranKemenag.ViewModels.Page;

public sealed partial class MushafPageViewModel : ViewModelBase
{
    private readonly MushafPageService _pageService;

    public MushafPageViewModel(MushafPageService pageService)
    {
        _pageService = pageService;

        selectedDisplayModeItem = DisplayModes.FirstOrDefault(
            m => m.Mode == DisplayMode.ComputerFont);
    }

    [ObservableProperty]
    private int pageNumber;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    /// <summary>
    /// Ringkasan mentah isi halaman yang sedang dimuat — dipakai untuk
    /// diagnosa manual (supaya bisa di-copy langsung dari UI, tanpa
    /// perlu buka Output window Visual Studio).
    /// </summary>
    [ObservableProperty]
    private string debugDump = string.Empty;

    /// <summary>
    /// Daftar mode tampilan — SAMA PERSIS dengan yang dipakai ReaderView
    /// (bukan daftar baru), supaya konsisten di seluruh aplikasi.
    /// </summary>
    public ObservableCollection<DisplayModeItem> DisplayModes { get; }
        = new(DisplayModeProvider.GetAll());

    [ObservableProperty]
    private DisplayModeItem? selectedDisplayModeItem;

    public ObservableCollection<PageLine> Lines { get; } = new();

    /// <summary>
    /// Dipanggil otomatis oleh CommunityToolkit.Mvvm setiap kali
    /// SelectedDisplayModeItem berubah (mis. user pilih Handwritten).
    /// Halaman yang sedang aktif otomatis dimuat ulang dengan mode baru.
    /// </summary>
    partial void OnSelectedDisplayModeItemChanged(DisplayModeItem? value)
    {
        if (PageNumber > 0)
        {
            _ = LoadPageAsync(PageNumber);
        }
    }

    public async Task LoadPageAsync(int pageNumberToLoad)
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var mode = SelectedDisplayModeItem?.Mode
                ?? DisplayMode.ComputerFont;

            var lines = await _pageService
                .GetPageAsync(pageNumberToLoad, mode)
                .ConfigureAwait(true);

            Lines.Clear();



            foreach (var line in lines)
            {
                Lines.Add(line);
            }

            PageNumber = pageNumberToLoad;

            var missingData = Lines.Any(
                l => l.Words.Any(w => w.DisplayText is null));

            if (missingData)
            {
                ErrorMessage = mode == DisplayMode.HandwrittenManuscript
                    ? "Sebagian/seluruh data Handwritten untuk halaman ini belum tersedia di msi_simbol.db."
                    : "Sebagian data Unicode untuk halaman ini tidak ditemukan.";
            }

            var dump = new System.Text.StringBuilder();
            dump.AppendLine($"Mode = {mode}");
            dump.AppendLine($"Page = {PageNumber}");
            dump.AppendLine($"Jumlah baris = {Lines.Count}");
            dump.AppendLine();

            dump.AppendLine("=== DIAGNOSTIK RAW TEKS ===");
            foreach (var diag in _pageService.LastDiagnostics)
            {
                dump.AppendLine(diag);
            }
            dump.AppendLine("=== AKHIR DIAGNOSTIK ===");
            dump.AppendLine();

            foreach (var line in Lines)
            {
                dump.AppendLine($"LINE {line.LineNumber}");

                foreach (var word in line.Words)
                {
                    var hex = string.IsNullOrEmpty(word.DisplayText)
                        ? "(KOSONG/NULL)"
                        : string.Join(
                            " ",
                            word.DisplayText.Select(c => $"U+{(int)c:X4}")
                        );

                    dump.AppendLine(
                        $"  [{word.WordOrder}] {word.Location} | " +
                        $"WordText='{word.WordText}' | " +
                        $"DisplayText='{word.DisplayText}' | " +
                        $"DisplayText HEX = {hex} | " +
                        $"KodeFont={word.KodeFont}");
                }
            }

            DebugDump = dump.ToString();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Gagal memuat halaman {pageNumberToLoad}: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}