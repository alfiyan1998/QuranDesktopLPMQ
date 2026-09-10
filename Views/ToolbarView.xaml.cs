using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace QuranKemenag.Views;

public sealed partial class ToolbarView : UserControl
{
    /// <summary>
    /// Dipicu saat tombol hamburger ditekan.
    /// </summary>
    public event RoutedEventHandler? SidebarToggleRequested;

    /// <summary>
    /// Dipicu saat tombol feature panel ditekan.
    /// </summary>
    public event RoutedEventHandler? PanelToggleRequested;

    /// <summary>
    /// Dipicu ketika user menekan tombol Copy.
    /// </summary>
    public event RoutedEventHandler? CopyRequested;

    /// <summary>
    /// Dipicu ketika user memilih Surah
    /// dari ComboBox Toolbar.
    /// </summary>
    public event SelectionChangedEventHandler?
        SurahSelectionRequested;

    public ToolbarView()
    {
        this.InitializeComponent();
    }

    private void SidebarToggleButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        SidebarToggleRequested?.Invoke(this, e);
    }

    private void PanelToggleButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        PanelToggleRequested?.Invoke(this, e);
    }

    private void SurahComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0)
            return;

        SurahSelectionRequested?.Invoke(
            this,
            e);
    }

    private void CopyButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        CopyRequested?.Invoke(this, e);
    }
}