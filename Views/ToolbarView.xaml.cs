using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
//using QuranKemenag.ViewModels;

namespace QuranKemenag.Views;

/// <summary>
/// Toolbar utama aplikasi. Di Tahap 2 ini, kontrol (ComboBox Surah, Ayah, dll)
/// belum diisi data apa pun. Yang aktif hanya dua event UI murni:
/// toggle Sidebar dan toggle Feature Panel. Belum ada logika bisnis di sini.
/// </summary>
public sealed partial class ToolbarView : UserControl
{
    /// <summary>Dipicu saat tombol hamburger (sidebar) ditekan.</summary>
    public event RoutedEventHandler? SidebarToggleRequested;

    /// <summary>Dipicu saat tombol feature panel ditekan.</summary>
    public event RoutedEventHandler? PanelToggleRequested;

    public ToolbarView()
    {
        this.InitializeComponent();
    }

    private void SidebarToggleButton_Click(object sender, RoutedEventArgs e)
    {
        SidebarToggleRequested?.Invoke(this, e);
    }

    private void PanelToggleButton_Click(object sender, RoutedEventArgs e)
    {
        PanelToggleRequested?.Invoke(this, e);
    }

    //private void AyahComboBox_SelectionChanged(
    //object sender,
    //SelectionChangedEventArgs e)
    //{
    //    if (DataContext is not MainViewModel vm)
    //        return;

    //    if (AyahComboBox.SelectedItem is int ayatNumber)
    //    {
    //        vm.SelectAyat(ayatNumber);
    //    }
    //}
}
