using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QuranKemenag.Models;

namespace QuranKemenag.Views;

public sealed partial class SurahView : UserControl
{
    private List<Surah> _allSurahs = new();

    public SurahView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Event ketika user memilih salah satu surah.
    /// </summary>
    public event EventHandler<Surah>? SurahSelected;

    public event EventHandler? JuzRequested;

    /// <summary>
    /// Mengisi daftar surah ke dalam tampilan.
    /// </summary>
    public void SetSurahs(IEnumerable<Surah> surahs)
    {
        _allSurahs = surahs.ToList();

        SurahItemsControl.ItemsSource = _allSurahs;
    }

    private void TxtCari_TextChanged(
    object sender,
    TextChangedEventArgs e)
    {
        var keyword = TxtCari.Text.Trim();

        if (string.IsNullOrWhiteSpace(keyword))
        {
            SurahItemsControl.ItemsSource = _allSurahs;
            return;
        }

        var hasil = _allSurahs
                .Where(s =>
                    (!string.IsNullOrWhiteSpace(s.LatinName) &&
                     s.LatinName.Contains(
                         keyword,
                         StringComparison.OrdinalIgnoreCase))

                    ||

                    (!string.IsNullOrWhiteSpace(s.Translation) &&
                     s.Translation.Contains(
                         keyword,
                         StringComparison.OrdinalIgnoreCase))

                    ||

                    (!string.IsNullOrWhiteSpace(s.ArabicName) &&
                     s.ArabicName.Contains(keyword))

                    ||

                    s.Id.ToString().Contains(keyword))
                .ToList();

        SurahItemsControl.ItemsSource = hasil;
    }

    private void JuzTab_Click(
    object sender,
    RoutedEventArgs e)
    {
        JuzRequested?.Invoke(
            this,
            EventArgs.Empty);
    }

    /// <summary>
    /// Menangani klik kartu surah.
    /// </summary>
    private void SurahCard_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.Tag is not Surah surah)
            return;

        SurahSelected?.Invoke(this, surah);
    }

    private void SurahCard_PointerEntered(
    object sender,
    Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is Border border)
        {
            border.Opacity = 0.75;
        }
    }


    private void SurahCard_PointerExited(
    object sender,
    Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is Border border)
        {
            border.Opacity = 1.0;
        }
    }

}