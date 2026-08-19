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

    public SidebarView()
    {
        this.InitializeComponent();
    }

    private void HomeItem_Tapped(object sender, TappedRoutedEventArgs e) => HomeClicked?.Invoke(this, EventArgs.Empty);
    private void SurahItem_Tapped(object sender, TappedRoutedEventArgs e) => SurahClicked?.Invoke(this, EventArgs.Empty);
    private void JuzItem_Tapped(object sender, TappedRoutedEventArgs e) => JuzClicked?.Invoke(this, EventArgs.Empty);
    private void PageItem_Tapped(object sender, TappedRoutedEventArgs e) => PageClicked?.Invoke(this, EventArgs.Empty);
    private void BookmarkItem_Tapped(object sender, TappedRoutedEventArgs e) => BookmarkClicked?.Invoke(this, EventArgs.Empty);
    private void SettingsItem_Tapped(object sender, TappedRoutedEventArgs e) => SettingsClicked?.Invoke(this, EventArgs.Empty);
}
