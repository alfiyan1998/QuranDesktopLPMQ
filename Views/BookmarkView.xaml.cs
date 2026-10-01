using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QuranKemenag.Models;
using QuranKemenag.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using QuranKemenag.ViewModels;
using System.Linq;
using System.Collections.Generic;

namespace QuranKemenag.Views;

public sealed partial class BookmarkView : UserControl
{
    private BookmarkService? _bookmarkService;
    private IReadOnlyList<Bookmark> _allBookmarks =
    Array.Empty<Bookmark>();
    public event EventHandler<Bookmark>? BookmarkOpened;
    public event EventHandler<Bookmark>? BookmarkDeleted;

    public BookmarkView()
    {
        InitializeComponent();

        BookmarkFilterComboBox.SelectionChanged +=
            BookmarkFilterComboBox_SelectionChanged;

        BookmarkSortComboBox.SelectionChanged +=
            BookmarkSortComboBox_SelectionChanged;

        _bookmarkService =
            App.Services?.GetService<BookmarkService>();

        Loaded += BookmarkView_Loaded;
    }

    private async void BookmarkView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        await RefreshBookmarksAsync();
    }

    public async Task RefreshBookmarksAsync()
    {
        if (_bookmarkService is null)
            return;

        try
        {
            var bookmarks =
                await _bookmarkService.GetAllAsync();

            var vm =
                App.Services?.GetService<MainViewModel>();

            if (vm is not null)
            {
                foreach (var bookmark in bookmarks)
                {
                    var surah =
                        vm.Surahs.FirstOrDefault(
                            s => s.Id == bookmark.Surah);

                    bookmark.SurahName =
                        surah?.LatinName
                        ?? $"Surah {bookmark.Surah}";
                }
            }

            _allBookmarks = bookmarks;

            BookmarkCountText.Text =
                bookmarks.Count == 1
                    ? "1 bookmark"
                    : $"{bookmarks.Count} bookmark";

            // Terapkan kembali Search, Filter, Sort,
            // sekaligus memperbarui Empty State.
            ApplyBookmarkFilterAndSort();

            Debug.WriteLine(
                $"[BOOKMARK VIEW] Jumlah bookmark = {bookmarks.Count}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"[BOOKMARK VIEW ERROR] {ex}");
        }
    }

    private void OpenBookmarkButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.Tag is not Bookmark bookmark)
            return;

        Debug.WriteLine(
            $"[BOOKMARK OPEN] " +
            $"Surah={bookmark.Surah}, " +
            $"Ayat={bookmark.Ayat}");

        BookmarkOpened?.Invoke(this, bookmark);

        // Navigasi akan kita sambungkan pada langkah berikutnya.
    }

    private async void EditBookmarkButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.Tag is not Bookmark bookmark)
            return;

        if (_bookmarkService is null)
            return;

        var noteTextBox = new TextBox
        {
            Text = bookmark.Note ?? string.Empty,
            PlaceholderText = "Tulis catatan...",
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 100
        };

        var contentPanel = new StackPanel
        {
            Spacing = 8
        };

        contentPanel.Children.Add(
            new TextBlock
            {
                Text = $"{bookmark.SurahName} • Ayat {bookmark.Ayat}",
                FontSize = 14
            });

        contentPanel.Children.Add(
            new TextBlock
            {
                Text = "Catatan",
                FontSize = 13
            });

        contentPanel.Children.Add(noteTextBox);

        var dialog = new ContentDialog
        {
            Title = "Edit Bookmark",
            Content = contentPanel,
            PrimaryButtonText = "Simpan",
            CloseButtonText = "Batal",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result != ContentDialogResult.Primary)
            return;

        string? note = noteTextBox.Text?.Trim();

        await _bookmarkService.UpdateNoteAsync(
            bookmark.Id,
            note);

        await RefreshBookmarksAsync();
    }

    private async void DeleteBookmarkButton_Click(
     object sender,
     RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.Tag is not Bookmark bookmark)
            return;

        if (_bookmarkService is null)
            return;

        var dialog = new ContentDialog
        {
            Title = "Hapus Bookmark",
            Content =
                $"Apakah Anda yakin ingin menghapus bookmark " +
                $"{bookmark.SurahName} • Ayat {bookmark.Ayat}?",
            PrimaryButtonText = "Hapus",
            CloseButtonText = "Batal",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result != ContentDialogResult.Primary)
            return;

        await _bookmarkService.DeleteAsync(bookmark.Id);

        await RefreshBookmarksAsync();

        BookmarkDeleted?.Invoke(this, bookmark);
    }

    private void BookmarkSearchBox_TextChanged(
    object sender,
    TextChangedEventArgs e)
    {
        ApplyBookmarkFilterAndSort();
    }

    private void BookmarkFilterComboBox_SelectionChanged(
    object sender,
    SelectionChangedEventArgs e)
    {
        ApplyBookmarkFilterAndSort();
    }

    private void BookmarkSortComboBox_SelectionChanged(
    object sender,
    SelectionChangedEventArgs e)
    {
        ApplyBookmarkFilterAndSort();
    }

    private void ApplyBookmarkFilterAndSort()
    {
        if (BookmarkFilterComboBox is null ||
            BookmarkSortComboBox is null)
        {
            return;
        }

        IEnumerable<Bookmark> result = _allBookmarks;

        // SEARCH
        string keyword =
            BookmarkSearchBox.Text?.Trim()
            ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            result = result.Where(bookmark =>
                bookmark.SurahName.Contains(
                    keyword,
                    StringComparison.OrdinalIgnoreCase)
                ||
                bookmark.Ayat.ToString().Contains(
                    keyword,
                    StringComparison.OrdinalIgnoreCase)
                ||
                (bookmark.Note?.Contains(
                    keyword,
                    StringComparison.OrdinalIgnoreCase)
                    ?? false));
        }

        // FILTER
        if (BookmarkFilterComboBox.SelectedItem
            is ComboBoxItem filterItem)
        {
            string filter =
                filterItem.Tag?.ToString()
                ?? "all";

            if (filter == "with-note")
            {
                result = result.Where(bookmark =>
                    !string.IsNullOrWhiteSpace(bookmark.Note));
            }
            else if (filter == "without-note")
            {
                result = result.Where(bookmark =>
                    string.IsNullOrWhiteSpace(bookmark.Note));
            }
        }

        // SORT
        if (BookmarkSortComboBox.SelectedItem
            is ComboBoxItem sortItem)
        {
            string sort =
                sortItem.Tag?.ToString()
                ?? "newest";

            if (sort == "newest")
            {
                result = result.OrderByDescending(
                    bookmark => bookmark.CreatedAt);
            }
            else if (sort == "oldest")
            {
                result = result.OrderBy(
                    bookmark => bookmark.CreatedAt);
            }
            else if (sort == "surah")
            {
                result = result
                    .OrderBy(bookmark => bookmark.Surah)
                    .ThenBy(bookmark => bookmark.Ayat);
            }
        }

        var filteredBookmarks = result.ToList();

        BookmarkListView.ItemsSource =
            filteredBookmarks;

        if (_allBookmarks.Count == 0)
        {
            EmptyBookmarkPanel.Visibility =
                Visibility.Visible;

            NoSearchResultPanel.Visibility =
                Visibility.Collapsed;
        }
        else if (filteredBookmarks.Count == 0)
        {
            EmptyBookmarkPanel.Visibility =
                Visibility.Collapsed;

            NoSearchResultPanel.Visibility =
                Visibility.Visible;
        }
        else
        {
            EmptyBookmarkPanel.Visibility =
                Visibility.Collapsed;

            NoSearchResultPanel.Visibility =
                Visibility.Collapsed;
        }
    }
}