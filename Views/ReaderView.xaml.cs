using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Extensions.DependencyInjection;

using QuranKemenag.Controls;
using QuranKemenag.Models;
using QuranKemenag.ViewModels;
using QuranKemenag.Services;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics;



namespace QuranKemenag.Views;

public sealed partial class ReaderView : UserControl
{
    private MainViewModel? _vm;
    private INotifyPropertyChanged? _vmNpc;
    private INotifyCollectionChanged? _ayatsCollection;

    private bool _navigationPending;
    private bool _settingsPending;
    private bool _layoutHandlerAttached;
    private bool _bringIntoViewPending;


    // =========================================================
    // CONSTRUCTOR
    // =========================================================

    public ReaderView()
    {
        InitializeComponent();

        Loaded += ReaderView_Loaded;
        Unloaded += ReaderView_Unloaded;
        DataContextChanged += ReaderView_DataContextChanged;
    }


    // =========================================================
    // LOADED
    // =========================================================

    private void ReaderView_Loaded(
        object? sender,
        RoutedEventArgs e)
    {
        AttachToDataContext(DataContext);

        // Terapkan setting awal.
        QueueApplySettings();

        // Jangan langsung scroll SelectedAyat ketika
        // Reader pertama kali tampil.
    }


    // =========================================================
    // UNLOADED
    // =========================================================

    private void ReaderView_Unloaded(
        object? sender,
        RoutedEventArgs e)
    {
        RemoveAyatCardHandlers();

        DetachFromViewModel();

        DetachLayoutHandler();
    }


    // =========================================================
    // DATA CONTEXT CHANGED
    // =========================================================

    private void ReaderView_DataContextChanged(
        FrameworkElement sender,
        DataContextChangedEventArgs args)
    {
        DetachFromViewModel();

        AttachToDataContext(args.NewValue);

        QueueApplySettings();

        // Tidak langsung melakukan navigasi di sini.
        // Navigasi dilakukan ketika SelectedAyat memang berubah.
    }


    // =========================================================
    // ATTACH VIEW MODEL
    // =========================================================

    private void AttachToDataContext(
        object? newDc)
    {
        if (newDc is not MainViewModel vm)
            return;

        _vm = vm;


        // -----------------------------------------------------
        // PropertyChanged
        // -----------------------------------------------------

        _vmNpc = vm;

        _vmNpc.PropertyChanged +=
            Vm_PropertyChanged;


        // -----------------------------------------------------
        // Ayats CollectionChanged
        // -----------------------------------------------------

        if (vm.Ayats is INotifyCollectionChanged collection)
        {
            _ayatsCollection = collection;

            _ayatsCollection.CollectionChanged +=
                Ayats_CollectionChanged;
        }
    }


    // =========================================================
    // DETACH VIEW MODEL
    // =========================================================

    private void DetachFromViewModel()
    {
        if (_vmNpc is not null)
        {
            _vmNpc.PropertyChanged -=
                Vm_PropertyChanged;

            _vmNpc = null;
        }


        if (_ayatsCollection is not null)
        {
            _ayatsCollection.CollectionChanged -=
                Ayats_CollectionChanged;

            _ayatsCollection = null;
        }


        _vm = null;
    }


    // =========================================================
    // AYATS COLLECTION CHANGED
    // =========================================================

    private void Ayats_CollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs e)
    {
        // Collection changed: schedule UI updates and handler attachment.
        // Tunggu sampai ListView membuat visual AyatCard.

        QueueApplySettings();

        QueueAttachAyatCardHandlers();

        // Schedule a layout diagnostic after UI has a chance to realize visuals.
            _ = DispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                // Diagnostic logs removed: [READER LAYOUT DEBUG]
            }
            catch (Exception)
            {
                // suppressed
            }
        });
    }


    // =========================================================
    // VIEW MODEL PROPERTY CHANGED
    // =========================================================

    private void Vm_PropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (_vm is null)
            return;


        // =====================================================
        // SELECTED AYAT
        // =====================================================

        if (e.PropertyName ==
            nameof(MainViewModel.SelectedAyat))
        {
            // Hanya update highlight.
            UpdateSelectedAyat();

            // Kemudian scroll.
            QueueNavigateToSelectedAyat();

            return;
        }


        // =====================================================
        // DISPLAY SETTINGS
        // =====================================================

        if (e.PropertyName ==
                nameof(MainViewModel.ArabicFontSize)
            || e.PropertyName ==
                nameof(MainViewModel.TranslationFontSize)
            || e.PropertyName ==
                nameof(MainViewModel.IsyaratFontSize)
            || e.PropertyName ==
                nameof(MainViewModel.ShowTranslation)
            || e.PropertyName ==
                nameof(MainViewModel.ShowIsyarat))
        {
            QueueApplySettings();
        }
    }


    // =========================================================
    // QUEUE APPLY SETTINGS
    // =========================================================

    private void QueueApplySettings()
    {
        if (_settingsPending)
            return;

        _settingsPending = true;


        _ = DispatcherQueue.TryEnqueue(
            () =>
            {
                _settingsPending = false;

                ApplySettingsToAyatCards();

                AttachAyatCardHandlers();
            });
    }


    // =========================================================
    // APPLY DISPLAY SETTINGS
    // =========================================================

    private void ApplySettingsToAyatCards()
    {
        if (_vm is null)
            return;

        if (PART_ItemsControl is null)
            return;

        // Apply settings to AyatCard visuals when available.
        // Diagnostic: count realized AyatCard controls
            try
            {
                var realized = FindAyatCards(PART_ItemsControl).ToList();
                // Diagnostic logs removed: [READER LAYOUT DEBUG] AyatCard realized count
            }
            catch (Exception)
            {
                // suppressed
            }


        var cards =
            FindAyatCards(
                PART_ItemsControl);


        foreach (var card in cards)
        {
            // =================================================
            // TERJEMAHAN + FOOTNOTE
            // =================================================

            card.ShowTranslation =
                _vm.ShowTranslation;


            // =================================================
            // BAHASA ISYARAT
            // =================================================

            card.ShowIsyarat =
                _vm.ShowIsyarat;


            // =================================================
            // FONT
            // =================================================

            card.ArabicFontSize =
                _vm.ArabicFontSize;

            card.TranslationFontSize =
                _vm.TranslationFontSize;

            card.IsyaratFontSize =
                _vm.IsyaratFontSize;
        }
    }


    // =========================================================
    // UPDATE SELECTED AYAT
    // =========================================================

    private void UpdateSelectedAyat()
    {
        if (_vm is null)
            return;

        if (PART_ItemsControl is null)
            return;


        var cards =
            FindAyatCards(
                PART_ItemsControl);


        foreach (var card in cards)
        {
            card.SelectedAyat =
                _vm.SelectedAyat;
        }
    }

    public async Task RefreshBookmarkStatusAsync(
    int surah,
    int ayatNumber)
    {
        var bookmarkService =
            App.Services?.GetService<BookmarkService>();

        if (bookmarkService is null)
            return;

        var bookmark =
            await bookmarkService.GetAsync(
                surah,
                ayatNumber);

        var cards =
            FindAyatCards(PART_ItemsControl);

        foreach (var card in cards)
        {
            if (card.DataContext is not Ayat ayat)
                continue;

            if (ayat.Surah != surah ||
                ayat.AyatNumber != ayatNumber)
            {
                continue;
            }

            ayat.IsBookmarked =
                bookmark is not null;

            card.IsBookmarked =
                bookmark is not null;

            break;
        }
    }


    // =========================================================
    // FIND AYAT CARDS
    // =========================================================

    private static IEnumerable<AyatCard> FindAyatCards(
        DependencyObject parent)
    {
        if (parent is null)
            yield break;


        var count =
            VisualTreeHelper.GetChildrenCount(
                parent);


        for (int i = 0; i < count; i++)
        {
            var child =
                VisualTreeHelper.GetChild(
                    parent,
                    i);


            if (child is AyatCard card)
            {
                yield return card;
            }


            foreach (var descendant in
                     FindAyatCards(child))
            {
                yield return descendant;
            }
        }
    }

    // =========================================================
    // FIND FIRST GRID (ROOT GRID) - safe diagnostic helper
    // =========================================================

    // FindFirstGrid diagnostic helper removed.


    // =========================================================
    // ATTACH CLICK HANDLERS
    // =========================================================

    private void AttachAyatCardHandlers()
    {
        if (PART_ItemsControl is null)
            return;


        var cards =
            FindAyatCards(
                PART_ItemsControl);


        foreach (var card in cards)
        {
            card.AyatSelected -=
                Card_AyatSelected;

            card.AyatSelected +=
                Card_AyatSelected;

            card.LastReadingChanged -=
       Card_LastReadingChanged;

            card.LastReadingChanged +=
                Card_LastReadingChanged;


            // Pastikan highlight juga sinkron.
            if (_vm is not null)
            {
                card.SelectedAyat =
                    _vm.SelectedAyat;
            }
        }
    }


    // =========================================================
    // QUEUE ATTACH HANDLERS
    // =========================================================

    private void QueueAttachAyatCardHandlers()
    {
        _ = DispatcherQueue.TryEnqueue(
            () =>
            {
                AttachAyatCardHandlers();

                // Jika ListView belum selesai membuat visual,
                // tunggu LayoutUpdated.
                AttachLayoutHandler();
            });
    }


    // =========================================================
    // AYAT CLICKED
    // =========================================================

    private void Card_AyatSelected(
        object? sender,
        Ayat ayat)
    {
        if (_vm is null)
            return;


        if (ReferenceEquals(
            _vm.SelectedAyat,
            ayat))
        {
            return;
        }


        // Hanya mengubah SelectedAyat.
        //
        // Jangan mengubah:
        // ShowTranslation
        // ShowIsyarat
        // FontSize

        _vm.SelectedAyat =
            ayat;
    }

    private void Card_LastReadingChanged(
    object? sender,
    EventArgs e)
    {
        if (PART_ItemsControl is null)
            return;

        var cards =
            FindAyatCards(PART_ItemsControl);

       

        try
        {
            using var key =
                Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"Software\QuranKemenag");

            if (key is null)
            {
                foreach (var card in cards)
                {
                    card.UpdateLastReadingIndicator();
                }

                return;
            }

            var surahValue =
                key.GetValue("LastReadingSurah");

            var ayatValue =
                key.GetValue("LastReadingAyat");

            if (surahValue is null || ayatValue is null)
            {
                foreach (var card in cards)
                {
                    card.UpdateLastReadingIndicator();
                }

                return;
            }

            int lastSurah =
                Convert.ToInt32(surahValue);

            int lastAyat =
                Convert.ToInt32(ayatValue);

            Debug.WriteLine(
                $"[LAST READING REFRESH] " +
                $"Registry={lastSurah}:{lastAyat}");

            foreach (var card in cards)
            {
                card.UpdateLastReadingIndicator(
                    lastSurah,
                    lastAyat);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                "[LAST READING REFRESH ERROR]");

            Debug.WriteLine(ex);
        }
    }

    // =========================================================
    // REMOVE CLICK HANDLERS
    // =========================================================

    private void RemoveAyatCardHandlers()
    {
        if (PART_ItemsControl is null)
            return;


        var cards =
            FindAyatCards(
                PART_ItemsControl);


        foreach (var card in cards)
        {
            card.AyatSelected -=
                Card_AyatSelected;

            card.LastReadingChanged -=
                Card_LastReadingChanged;
        }
    }


    // =========================================================
    // QUEUE NAVIGATION
    // =========================================================

    private void QueueNavigateToSelectedAyat()
    {
        if (_navigationPending)
            return;

        if (_vm?.SelectedAyat is null)
            return;


        _navigationPending = true;


        _ = DispatcherQueue.TryEnqueue(
            () =>
            {
                _navigationPending = false;

                NavigateToSelectedAyat();
            });
    }


    // =========================================================
    // NAVIGATE SELECTED AYAT
    // =========================================================

    private void NavigateToSelectedAyat()
    {
        if (_vm?.SelectedAyat is not Ayat target)
            return;


        if (PART_ItemsControl is null)
            return;


        // If the target is not part of the current reader context, we cannot
        // scroll to it here. The ViewModel is responsible for loading the
        // appropriate context (Juz) and will set SelectedAyat again when ready.
        if (!_vm.Ayats.Contains(target))
            return;

        // Ensure the item container has been realized. If virtualization delayed
        // realization, attach a LayoutUpdated handler to retry scrolling once
        // the ListView has measured its items.
        PART_ItemsControl.ScrollIntoView(
            target,
            ScrollIntoViewAlignment.Leading);

        // Try to obtain the realized container immediately. If virtualization
        // prevented realization, schedule a layout handler to finish BringIntoView
        // once the item container is realized.
        var container = PART_ItemsControl.ContainerFromItem(target) as ListViewItem;
        if (container is not null)
        {
            // If container is realized, ensure it's fully visible by calling ScrollIntoView
            // again which can correct minor offsets caused by virtualization.
            PART_ItemsControl.ScrollIntoView(target, ScrollIntoViewAlignment.Leading);

            // Ensure handlers are attached as well.
            QueueAttachAyatCardHandlers();
            return;
        }

        _bringIntoViewPending = true;
        // Attach a layout handler to retry when layout updates.
        AttachLayoutHandler();
    }


    // =========================================================
    // LAYOUT HANDLER
    // =========================================================

    private void AttachLayoutHandler()
    {
        if (_layoutHandlerAttached)
            return;

        if (PART_ItemsControl is null)
            return;


        PART_ItemsControl.LayoutUpdated +=
            ItemsControl_LayoutUpdated;


        _layoutHandlerAttached = true;
    }


    // =========================================================
    // DETACH LAYOUT HANDLER
    // =========================================================

    private void DetachLayoutHandler()
    {
        if (!_layoutHandlerAttached)
            return;


        if (PART_ItemsControl is not null)
        {
            PART_ItemsControl.LayoutUpdated -=
                ItemsControl_LayoutUpdated;
        }


        _layoutHandlerAttached = false;
    }


    // =========================================================
    // LAYOUT UPDATED
    // =========================================================

    private void ItemsControl_LayoutUpdated(
        object? sender,
        object e)
    {
        // Pastikan event click sudah terpasang.
        AttachAyatCardHandlers();

        // If a previous navigation attempted to ScrollIntoView but the
        // item container was not yet realized due to virtualization, try
        // to obtain the container now and call BringIntoView to ensure the
        // item is visible. Do not rely on arbitrary delays; rely on
        // LayoutUpdated which signals the layout pass.
        if (_bringIntoViewPending && _vm?.SelectedAyat is Ayat target)
        {
            var container = PART_ItemsControl.ContainerFromItem(target) as ListViewItem;
            if (container is not null)
            {
                // Use ScrollIntoView again when the container becomes available to
                // ensure the item is visible. This avoids using BringIntoView API
                // which may not be present in the target framework.
                PART_ItemsControl.ScrollIntoView(target, ScrollIntoViewAlignment.Leading);

                _bringIntoViewPending = false;
            }
        }

        // Detach handler if no further retries are pending.
        if (!_bringIntoViewPending)
            DetachLayoutHandler();
    }

    private void PART_ItemsControl_ItemClick(
    object sender,
    ItemClickEventArgs e)
    {
        if (_vm is null)
            return;

        if (e.ClickedItem is not Ayat ayat)
            return;

        _vm.SelectedAyat = ayat;
    }
}