using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QuranKemenag.Models;
using QuranKemenag.Services.Font;
using QuranKemenag.ViewModels.Page;
using System.ComponentModel;
using System.Linq;

namespace QuranKemenag.Views;

public sealed partial class MushafPerHalamanView : UserControl
{
    public MushafPageViewModel ViewModel { get; }

    public MushafPerHalamanView()
    {
        InitializeComponent();

        ViewModel = App.Services.GetRequiredService<MushafPageViewModel>();
        DataContext = ViewModel;

        ViewModel.PropertyChanged += ViewModel_PropertyChanged;

        Loaded += MushafPerHalamanView_Loaded;
    }

    private async void MushafPerHalamanView_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadPageAsync(3);
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Setiap kali proses load selesai (initial load ATAU ganti mode),
        // terapkan ulang font yang sesuai untuk mode yang sedang aktif.
        if (e.PropertyName == nameof(MushafPageViewModel.IsLoading) &&
            !ViewModel.IsLoading)
        {
            ApplyFontForCurrentMode();
        }
    }

    private void ApplyFontForCurrentMode()
    {
        var fontManager = App.Services.GetRequiredService<IFontManager>();
        var mode = ViewModel.SelectedDisplayModeItem?.Mode ?? DisplayMode.ComputerFont;

        if (mode == DisplayMode.ComputerFont)
        {
            Root.FontFamily = fontManager.GetUnicodeFont();
            return;
        }

        // Mode Handwritten: cari kode_font pertama yang benar-benar
        // punya data di halaman ini (bisa saja belum ada sama sekali).
        var kodeFont = ViewModel.Lines
            .SelectMany(l => l.Words)
            .Select(w => w.KodeFont)
            .FirstOrDefault(k => k is > 0);

        Root.FontFamily = kodeFont is > 0
            ? fontManager.GetSymbolFont(kodeFont.Value)
            : fontManager.GetUnicodeFont(); // fallback: belum ada data
    }

    private void CopyDebugButton_Click(object sender, RoutedEventArgs e)
    {
        var package = new Windows.ApplicationModel.DataTransfer.DataPackage();
        package.SetText(ViewModel.DebugDump);
        Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(package);
    }
}