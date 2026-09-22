using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace QuranKemenag.Views;

/// <summary>
/// Sidebar navigasi aplikasi. Menyediakan event sederhana yang di-wire oleh MainWindow.
/// </summary>
public sealed partial class SidebarView : UserControl
{
    public event EventHandler? HomeClicked;
    public event EventHandler? SurahClicked;
    public event EventHandler? JuzClicked;
    public event EventHandler? PageClicked;
    public event EventHandler? BookmarkClicked;
    public event EventHandler? SettingsClicked;

    public event RoutedEventHandler? SidebarToggleRequested;

    public SidebarView()
    {
        this.InitializeComponent();
    }



    public void SetPaneOpen(bool isOpen)
    {
        ExpandedPanel.Visibility =
        isOpen
            ? Visibility.Visible
            : Visibility.Collapsed;

        CompactPanel.Visibility =
            isOpen
                ? Visibility.Collapsed
                : Visibility.Visible;
    }



    private void SidebarToggleButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        SidebarToggleRequested?.Invoke(this, e);
    }

    private void HomeButton_Click(object sender, RoutedEventArgs e)
    {
        HomeClicked?.Invoke(this, EventArgs.Empty);
    }

    private void SurahButton_Click(object sender, RoutedEventArgs e)
    {
        SurahClicked?.Invoke(this, EventArgs.Empty);
    }

    private void JuzButton_Click(object sender, RoutedEventArgs e)
    {
        JuzClicked?.Invoke(this, EventArgs.Empty);
    }

    private void PageButton_Click(object sender, RoutedEventArgs e)
    {
        PageClicked?.Invoke(this, EventArgs.Empty);
    }

    private void BookmarkButton_Click(object sender, RoutedEventArgs e)
    {
        BookmarkClicked?.Invoke(this, EventArgs.Empty);
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        SettingsClicked?.Invoke(this, EventArgs.Empty);
    }

    
}
