using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QuranKemenag.Models;
using QuranKemenag.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace QuranKemenag.Views;

public sealed partial class JuzView : Page
{
    private readonly JuzRepository _juzRepository;

    public event EventHandler<JuzInfo>? JuzSelected;

    public JuzView()
    {
        InitializeComponent();

        _juzRepository =
            App.Services.GetRequiredService<JuzRepository>();

        Loaded += JuzView_Loaded;
    }

    private async void JuzView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        await LoadJuzAsync();
    }

    private async Task LoadJuzAsync()
    {
        try
        {
            LoadingRing.IsActive = true;
            LoadingText.Text = "Memuat daftar Juz...";

            IReadOnlyList<JuzInfo> juzList =
                await _juzRepository.GetAllJuzInfoAsync();

            JuzGrid.ItemsSource = juzList;

            LoadingText.Text =
                $"{juzList.Count} Juz tersedia.";

            Debug.WriteLine(
                $"[JUZ] Jumlah Juz : {juzList.Count}");
        }
        catch (Exception ex)
        {
            LoadingText.Text =
                "Gagal memuat data Juz.";

            Debug.WriteLine(
                $"[JUZ ERROR] {ex}");
        }
        finally
        {
            LoadingRing.IsActive = false;
        }
    }

    private void JuzGrid_ItemClick(
        object sender,
        ItemClickEventArgs e)
    {
        if (e.ClickedItem is not JuzInfo juz)
            return;

        Debug.WriteLine(
            $"[JUZ NAVIGATION] " +
            $"Juz={juz.JuzNumber} " +
            $"Surah={juz.SurahNumber} " +
            $"Ayat={juz.AyatNumber}");

        JuzSelected?.Invoke(this, juz);
    }
}