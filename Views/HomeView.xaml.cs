using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QuranKemenag.Models;
using QuranKemenag.Services;
using QuranKemenag.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace QuranKemenag.Views;

public sealed partial class HomeView : UserControl
{
    private BookmarkService? _bookmarkService;

    public HomeView()
    {
        this.InitializeComponent();
        Loaded += HomeView_Loaded;
        _bookmarkService = App.Services?.GetService<BookmarkService>();
    }

    private void LoadLastReadingPosition()
    {
        try
        {
            using var key =
                Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"Software\QuranKemenag");

            if (key is null)
            {
                LastReadingSurahText.Text =
                    "Belum ada bacaan";

                LastReadingAyatText.Text =
                    "Mulai membaca Al-Qur'an";

                return;
            }

            var surahValue =
                key.GetValue("LastReadingSurah");

            var ayatValue =
                key.GetValue("LastReadingAyat");

            if (surahValue is null ||
                ayatValue is null)
            {
                LastReadingSurahText.Text =
                    "Belum ada bacaan";

                LastReadingAyatText.Text =
                    "Mulai membaca Al-Qur'an";

                return;
            }

            int surahNumber =
                Convert.ToInt32(surahValue);

            int ayatNumber =
                Convert.ToInt32(ayatValue);

            var vm =
                App.Services?.GetService<MainViewModel>();

            if (vm is null)
                return;

            var surah =
                vm.Surahs.FirstOrDefault(
                    s => s.Id == surahNumber);

            LastReadingSurahText.Text =
                surah?.LatinName
                ?? $"Surah {surahNumber}";

            LastReadingAyatText.Text =
                $"Ayat {ayatNumber}";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                "[HOME LAST READING ERROR]");

            System.Diagnostics.Debug.WriteLine(ex);

            LastReadingSurahText.Text =
                "Belum ada bacaan";

            LastReadingAyatText.Text =
                "Mulai membaca Al-Qur'an";
        }
    }

    private void HomeView_Loaded(
    object sender,
    RoutedEventArgs e)
    {
        LoadLastReadingPosition();
        _ = RefreshBookmarkCountAsync();
    }

    private async void ContinueReadingButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        try
        {
            using var key =
                Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"Software\QuranKemenag");

            if (key is null)
                return;

            var surahValue =
                key.GetValue("LastReadingSurah");

            var ayatValue =
                key.GetValue("LastReadingAyat");

            if (surahValue is null ||
                ayatValue is null)
            {
                return;
            }

            int surahNumber =
                Convert.ToInt32(surahValue);

            int ayatNumber =
                Convert.ToInt32(ayatValue);

            var vm =
                App.Services?.GetService<MainViewModel>();

            if (vm is null)
                return;

            var surah =
                vm.Surahs.FirstOrDefault(
                    s => s.Id == surahNumber);

            if (surah is null)
                return;

            Debug.WriteLine(
                $"[HOME CONTINUE] " +
                $"Surah={surahNumber}, " +
                $"Ayat={ayatNumber}");

            // Pilih Surah menggunakan mekanisme
            // yang sudah digunakan Toolbar.
            vm.SelectedSurah = surah;

            vm.DisplayViewMode =
                QuranDisplayMode.Ayat;

            // Tunggu proses pemuatan Surah selesai.
            await (
                vm.LastHandleSelectedSurahTask
                ?? Task.CompletedTask);

            // Pilih ayat terakhir yang tersimpan.
            vm.SelectedAyatNumber = ayatNumber;

            var ayat =
                vm.Ayats.FirstOrDefault(
                    a => a.Surah == surahNumber &&
                         a.AyatNumber == ayatNumber);

            if (ayat is not null)
            {
                vm.SelectedAyat = ayat;
            }

            Debug.WriteLine(
                $"[HOME CONTINUE] " +
                $"Reader menuju {surah.LatinName} " +
                $"Ayat {ayatNumber}");

            // Ensure Reader view is displayed when continuing reading from Home.
            var mainWindow = QuranKemenag.App.MainAppWindow;
            mainWindow?.ShowReaderView();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                "[HOME CONTINUE ERROR]");

            Debug.WriteLine(ex);
        }
    }

    public void RefreshLastReadingPosition()
    {
        LoadLastReadingPosition();
        _ = RefreshBookmarkCountAsync();
    }

    public async Task RefreshBookmarkCountAsync()
    {
        try
        {
            if (_bookmarkService is null)
            {
                BookmarkCountText.Text = "0";
                return;
            }

            var list = await _bookmarkService.GetAllAsync();

            int count = list?.Count ?? 0;

            Debug.WriteLine($"[HOME BOOKMARK] Jumlah bookmark = {count}");

            BookmarkCountText.Text = count.ToString();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[HOME BOOKMARK ERROR]");
            Debug.WriteLine(ex);
            BookmarkCountText.Text = "0";
        }
    }
}
