using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QuranKemenag.ViewModels;

namespace QuranKemenag.Views;

/// <summary>
/// Panel fitur di sisi kanan aplikasi, berisi section Copy, Search the Ayahs,
/// Advanced Search, dan Options (menggunakan Expander). Di Tahap 2, isi tiap
/// section masih placeholder teks — komponen sungguhan ditambahkan di Tahap 9 & 10.
/// </summary>
public sealed partial class FeaturePanelView : UserControl
{
    public FeaturePanelView()
    {
        this.InitializeComponent();
    }

    private void PresetComboBox_SelectionChanged(
    object sender,
    SelectionChangedEventArgs e)
    {
        if (PresetComboBox.SelectedItem is not ComboBoxItem item)
            return;

        if (DataContext is not MainViewModel vm)
            return;

        if (item.Content is string preset)
        {
            vm.ApplyDisplayPreset(preset);
        }
    }
}
