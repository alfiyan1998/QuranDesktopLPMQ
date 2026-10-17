using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QuranKemenag.ViewModels;
using Microsoft.UI.Xaml.Media;
using System.Linq;
using System;
using Microsoft.UI.Dispatching;

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

        // Abaikan perubahan yang berasal dari kode (sinkronisasi Juz/Ayat).
        // Perubahan dari user selalu terjadi saat ComboBox terbuka / fokus.
        if (sender is ComboBox cb &&
            !cb.IsDropDownOpen &&
            cb.FocusState == FocusState.Unfocused)
        {
            return;
        }

        SurahSelectionRequested?.Invoke(
            this,e);
    }

    private void CopyButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        //sebelum
        //CopyRequested?.Invoke(this, e);
        //sesudah
        if (!HasOpenedContent)
        {
            // Tombol Copy juga ada di flyout overflow (jendela sempit),
            // jadi arahkan balon ke tombol yang terlihat.
            FrameworkElement target =
                ReferenceEquals(sender, CopyButton)
                    ? CopyButton
                    : OverflowButton;

            ShowSelectFirstTip(target);
            return;
        }

                CopyRequested?.Invoke(this, e);
            }

    /// <summary>
    /// True jika sudah ada Surah, ayat, atau halaman yang dibuka.
    /// </summary>
    //private bool HasOpenedContent =>
    //    DataContext is MainViewModel vm &&
    //    (vm.SelectedSurah is not null ||
    //     vm.SelectedAyat is not null ||
    //     vm.CurrentPage is not null);
    //edit mulai
    private bool _isReaderActive;

    /// <summary>
    /// Dipanggil MainWindow setiap kali halaman berganti.
    /// </summary>
    public void SetReaderActive(bool isActive)
    {
        _isReaderActive = isActive;
    }

    /// <summary>
    /// True jika Reader sedang tampil.
    /// </summary>
    private bool HasOpenedContent => _isReaderActive;
    //edit selesai

    //private void ShowSelectFirstTip(FrameworkElement target)
    //        {
    //            SelectFirstTip.Target = target;
    //            SelectFirstTip.IsOpen = true;
    //        }

    //        private void AyahComboBox_DropDownOpened(
    //            object? sender,
    //            object e)
    //        {
    //            if (HasOpenedContent)
    //                return;

    //            // Jangan biarkan daftar kosong terbuka.
    //            AyahComboBox.IsDropDownOpen = false;

    //            ShowSelectFirstTip(AyahComboBox);
    //        }
    //edit mulai agar tidak bug di persurah dan perjuz
    private DispatcherQueueTimer? _tipTimer;

    private void ShowSelectFirstTip(FrameworkElement target)
    {
        SelectFirstTip.Target = target;
        SelectFirstTip.IsOpen = true;

        // Tutup otomatis setelah beberapa detik.
        _tipTimer ??= CreateTipTimer();
        _tipTimer.Stop();
        _tipTimer.Start();
    }

    private DispatcherQueueTimer CreateTipTimer()
    {
        var timer = DispatcherQueue.CreateTimer();
        timer.Interval = TimeSpan.FromSeconds(3);   // ubah durasi di sini
        timer.IsRepeating = false;
        timer.Tick += (_, _) => SelectFirstTip.IsOpen = false;
        return timer;
    }
    //edit selesai

    //private void GuardedComboBox_DropDownOpened(object? sender, object e)
    //{
    //    if (HasOpenedContent || sender is not ComboBox cb)
    //        return;

    //    cb.IsDropDownOpen = false;

    //    FrameworkElement target =
    //        ReferenceEquals(sender, DisplayModeComboBox) ||
    //        ReferenceEquals(sender, DisplayViewModeComboBox)
    //            ? cb
    //            : OverflowButton;   // combo berada di flyout

    //    ShowSelectFirstTip(target);
    //}
    //edit mulai agar tidak bug di persurah dan perjuz
    private void GuardedComboBox_DropDownOpened(object? sender, object e)
    {
        if (HasOpenedContent || sender is not ComboBox cb)
            return;

        // Tentukan elemen yang dijadikan target balon.
        FrameworkElement target =
            ReferenceEquals(sender, DisplayModeComboBox) ||
            ReferenceEquals(sender, DisplayViewModeComboBox) ||
            ReferenceEquals(sender, AyahComboBox)
                ? cb
                : OverflowButton;   // combo berada di flyout

        // Tahap 1: tutup dropdown SETELAH event ini selesai.
        cb.DispatcherQueue.TryEnqueue(() =>
        {
            cb.IsDropDownOpen = false;

            // Tahap 2: tampilkan balon setelah popup benar-benar tertutup.
            cb.DispatcherQueue.TryEnqueue(
                Microsoft.UI.Dispatching.DispatcherQueuePriority.Low,
                () => ShowSelectFirstTip(target));
        });
    }
    //edit seelsai

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

    private void SurahComboBox_DropDownOpened(object? sender, object e)
    {
        // Jika sudah ada surah terpilih, biarkan ComboBox scroll ke item itu.
        if (sender is not ComboBox cb || cb.SelectedItem is not null)
            return;

        // Belum ada surah terpilih -> mulai dari Al-Fatihah (paling atas).
        cb.DispatcherQueue.TryEnqueue(() =>
        {
            var popup = VisualTreeHelper
                .GetOpenPopupsForXamlRoot(cb.XamlRoot)
                .FirstOrDefault();

            var scrollViewer = FindDescendant<ScrollViewer>(popup?.Child);
            scrollViewer?.ChangeView(null, 0, null, true);
        });
    }

    private static T? FindDescendant<T>(DependencyObject? root)
        where T : DependencyObject
    {
        if (root is null)
            return null;

        int count = VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);

            if (child is T match)
                return match;

            var result = FindDescendant<T>(child);
            if (result is not null)
                return result;
        }

        return null;
    }
}