using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QuranKemenag.Models;
using QuranKemenag.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace QuranKemenag.Views;

public sealed partial class JuzView : Page
{
    private readonly JuzRepository _juzRepository;

    private IReadOnlyList<JuzInfo> _allJuz = Array.Empty<JuzInfo>();

    public event EventHandler<JuzInfo>? JuzSelected;

    public event EventHandler? SurahRequested;

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

            _allJuz = juzList;

            JuzGrid.ItemsSource = _allJuz;

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

    private void TxtCari_TextChanged(
    object sender,
    TextChangedEventArgs e)
    {
        var kata = TxtCari.Text.Trim();

        if (string.IsNullOrWhiteSpace(kata))
        {
            JuzGrid.ItemsSource = _allJuz;
            return;
        }

        var hasil = _allJuz
            .Where(j =>
                j.JuzNumber.ToString().Contains(kata) ||
                j.SurahName.Contains(
                    kata,
                    StringComparison.OrdinalIgnoreCase) ||
                j.StartInfo.Contains(
                    kata,
                    StringComparison.OrdinalIgnoreCase))
            .ToList();

        JuzGrid.ItemsSource = hasil;
    }

    private void SurahTab_Click(
    object sender,
    RoutedEventArgs e)
    {
        SurahRequested?.Invoke(
            this,
            EventArgs.Empty);
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

    private void JuzCard_PointerEntered(
    object sender,
    Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is Border border)
        {
            border.Opacity = 0.75;
        }
    }

    private void JuzCard_PointerExited(
        object sender,
        Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is Border border)
        {
            border.Opacity = 1.0;
        }
    }
}