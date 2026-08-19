using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QuranKemenag.Models;

namespace QuranKemenag.Views;

public sealed partial class SurahView : UserControl
{
    public SurahView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Event ketika user memilih salah satu surah.
    /// </summary>
    public event EventHandler<Surah>? SurahSelected;

    /// <summary>
    /// Mengisi daftar surah ke dalam tampilan.
    /// </summary>
    public void SetSurahs(IEnumerable<Surah> surahs)
    {
        SurahItemsControl.ItemsSource = surahs;
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
}