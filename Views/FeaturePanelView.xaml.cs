//using Microsoft.UI.Xaml;
//using Microsoft.UI.Xaml.Controls;
//using QuranKemenag.ViewModels;

//namespace QuranKemenag.Views;

///// <summary>
///// Panel fitur di sisi kanan aplikasi, berisi section Copy, Search the Ayahs,
///// Advanced Search, dan Options (menggunakan Expander). Di Tahap 2, isi tiap
///// section masih placeholder teks — komponen sungguhan ditambahkan di Tahap 9 & 10.
///// </summary>
//public sealed partial class FeaturePanelView : UserControl
//{
//    public FeaturePanelView()
//    {
//        this.InitializeComponent();
//    }

//    private void PresetComboBox_SelectionChanged(
//    object sender,
//    SelectionChangedEventArgs e)
//    {
//        if (PresetComboBox.SelectedItem is not ComboBoxItem item)
//            return;

//        if (DataContext is not MainViewModel vm)
//            return;

//        if (item.Content is string preset)
//        {
//            vm.ApplyDisplayPreset(preset);
//        }
//    }
//}

//edit untuk menambahkan fitur lock overlay dan tip agar pengguna tahu harus membuka Surah/Ayat/Halaman dulu sebelum mengakses panel fitur
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using QuranKemenag.ViewModels;
using Microsoft.UI.Dispatching;

namespace QuranKemenag.Views;

/// <summary>
/// Panel fitur di sisi kanan aplikasi. Pengaturan tampilan dikunci
/// sampai ada Surah/Juz/halaman yang dibuka.
/// </summary>
public sealed partial class FeaturePanelView : UserControl
{
    private MainViewModel? _vm;

    public FeaturePanelView()
    {
        this.InitializeComponent();

        DataContextChanged += FeaturePanelView_DataContextChanged;
    }

    /// <summary>
    /// True jika sudah ada Surah, ayat, atau halaman yang dibuka.
    /// Aturannya sama dengan ToolbarView.
    /// </summary>
    //private bool HasOpenedContent =>
    //    _vm is not null &&
    //    (_vm.SelectedSurah is not null ||
    //     _vm.SelectedAyat is not null ||
    //     _vm.CurrentPage is not null);

    //edit untuk menambahkan fitur lock overlay dan tip agar pengguna tahu harus membuka Surah/Ayat/Halaman dulu sebelum mengakses panel fitur
    private bool _isReaderActive;

    /// <summary>
    /// Dipanggil MainWindow setiap kali halaman berganti.
    /// </summary>
    public void SetReaderActive(bool isActive)
    {
        _isReaderActive = isActive;
        UpdateLockState();
    }

    private bool HasOpenedContent => _isReaderActive;
    //edit selesai


    private void FeaturePanelView_DataContextChanged(
        FrameworkElement sender,
        DataContextChangedEventArgs args)
    {
        if (_vm is not null)
            _vm.PropertyChanged -= Vm_PropertyChanged;

        _vm = args.NewValue as MainViewModel;

        if (_vm is not null)
            _vm.PropertyChanged += Vm_PropertyChanged;

        UpdateLockState();
    }

    private void Vm_PropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName) ||
            e.PropertyName == nameof(MainViewModel.SelectedSurah) ||
            e.PropertyName == nameof(MainViewModel.SelectedAyat) ||
            e.PropertyName == nameof(MainViewModel.CurrentPage))
        {
            DispatcherQueue.TryEnqueue(UpdateLockState);
        }
    }

    private void UpdateLockState()
    {
        bool locked = !HasOpenedContent;

        LockOverlay.Visibility =
            locked ? Visibility.Visible : Visibility.Collapsed;

        // Abu-abu sebagai petunjuk visual bahwa panel belum bisa dipakai.
        //SettingsPanel.IsEnabled = !locked;

        //edit: SettingsPanel.IsEnabled = true; // tetap bisa diakses, tapi overlay menutupi
        SettingsHost.IsEnabled = !locked;   
        //edit selesai
        if (!locked)
            SelectFirstTip.IsOpen = false;
    }

    //private void LockOverlay_Tapped(
    //    object sender,
    //    TappedRoutedEventArgs e)
    //{
    //    SelectFirstTip.Target = SettingsPanel;
    //    SelectFirstTip.IsOpen = true;
    //    e.Handled = true;
    //}

    //edit menambahkan timer untuk menutup tip otomatis
    private DispatcherQueueTimer? _tipTimer;

    private void LockOverlay_Tapped(
        object sender,
        TappedRoutedEventArgs e)
    {
        SelectFirstTip.Target = SettingsPanel;
        SelectFirstTip.IsOpen = true;
        e.Handled = true;

        // Tutup otomatis setelah beberapa detik.
        if (_tipTimer is null)
        {
            _tipTimer = DispatcherQueue.CreateTimer();
            _tipTimer.Interval = TimeSpan.FromSeconds(3);   // ubah durasi di sini
            _tipTimer.IsRepeating = false;
            _tipTimer.Tick += (_, _) => SelectFirstTip.IsOpen = false;
        }

        _tipTimer.Stop();
        _tipTimer.Start();
    }
    //edit selesai: menambahkan timer untuk menutup tip otomatis

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
//edit selesai
