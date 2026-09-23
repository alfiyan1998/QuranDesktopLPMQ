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

    private void ToolbarGrid_SizeChanged(
    object sender,
    SizeChangedEventArgs e)
    {
        
    }

    public void SetResponsiveState(bool isWide, bool isCompact)
    {
        if (isWide)
        {
            ModePanel.Visibility = Visibility.Visible;
            ModeTampilanPanel.Visibility = Visibility.Visible;
            OverflowButton.Visibility = Visibility.Collapsed;

            SurahComboBox.Visibility = Visibility.Visible;
            AyahComboBox.Visibility = Visibility.Visible;
            CopyButton.Visibility = Visibility.Visible;
        }
        else if (isCompact)
        {
            ModePanel.Visibility = Visibility.Collapsed;
            ModeTampilanPanel.Visibility = Visibility.Collapsed;
            OverflowButton.Visibility = Visibility.Visible;

            SurahComboBox.Visibility = Visibility.Collapsed;
            AyahComboBox.Visibility = Visibility.Collapsed;
            CopyButton.Visibility = Visibility.Collapsed;
        }
        else
        {
            ModePanel.Visibility = Visibility.Collapsed;
            ModeTampilanPanel.Visibility = Visibility.Collapsed;
            OverflowButton.Visibility = Visibility.Visible;

            SurahComboBox.Visibility = Visibility.Visible;
            AyahComboBox.Visibility = Visibility.Visible;
            CopyButton.Visibility = Visibility.Visible;
        }
    }
}