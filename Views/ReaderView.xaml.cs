using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

using QuranKemenag.Controls;
using QuranKemenag.Models;
using QuranKemenag.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace QuranKemenag.Views;

public sealed partial class ReaderView : UserControl
{
    private MainViewModel? _vm;
    private INotifyPropertyChanged? _vmNpc;
    private INotifyCollectionChanged? _ayatsCollection;

    private bool _navigationPending;
    private bool _settingsPending;
    private bool _layoutHandlerAttached;


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
        // Tunggu sampai ListView membuat visual AyatCard.

        QueueApplySettings();

        QueueAttachAyatCardHandlers();
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


        if (!_vm.Ayats.Contains(target))
            return;


        // =====================================================
        // LISTVIEW SUDAH MENDUKUNG SCROLLINTOView
        // =====================================================

        PART_ItemsControl.ScrollIntoView(
            target,
            ScrollIntoViewAlignment.Leading);


        DetachLayoutHandler();
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


        // Tidak melakukan Scroll di sini.
        //
        // Scroll hanya dilakukan ketika SelectedAyat
        // benar-benar berubah.
        //
        // Ini mencegah kasus:
        //
        // Ayat 7 dipilih
        // ↓
        // LayoutUpdated
        // ↓
        // Scroll kembali ke atas.
        //
        // Bug tersebut dihilangkan dengan sengaja.

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