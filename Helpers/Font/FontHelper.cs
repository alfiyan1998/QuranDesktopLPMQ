using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using QuranKemenag.Models;
using QuranKemenag.Services.Font;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace QuranKemenag.Helpers.Font;

/// <summary>
/// Attached Property untuk mengatur FontFamily secara otomatis
/// berdasarkan KodeFont pada Ayat.
///
/// KodeFont:
/// 0  = Unicode
/// >0 = Handwritten / Symbol
///
/// Compatible dengan ListView virtualization / recycling.
/// </summary>
[SupportedOSPlatform("windows10.0.17763.0")]
public static class FontHelper
{
    // =========================================================
    // AUTO FONT PROPERTY
    // =========================================================

    public static readonly DependencyProperty AutoFontProperty =
        DependencyProperty.RegisterAttached(
            "AutoFont",
            typeof(bool),
            typeof(FontHelper),
            new PropertyMetadata(
                false,
                OnAutoFontChanged));


    public static bool GetAutoFont(
        DependencyObject obj)
    {
        return (bool)obj.GetValue(
            AutoFontProperty);
    }


    public static void SetAutoFont(
        DependencyObject obj,
        bool value)
    {
        obj.SetValue(
            AutoFontProperty,
            value);
    }


    // =========================================================
    // FONT CACHE
    // =========================================================
    //
    // 0 = Unicode
    // 1,2,3,... = Symbol / Handwritten
    //
    // FontFamily dibuat satu kali kemudian digunakan kembali.
    // =========================================================

    private static readonly Dictionary<int, FontFamily>
        FontCache = new();


    // =========================================================
    // AUTO FONT CHANGED
    // =========================================================

    private static void OnAutoFontChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBlock tb)
            return;


        // =====================================================
        // DISABLE
        // =====================================================

        if (e.NewValue is not bool enabled ||
            !enabled)
        {
            tb.Loaded -= TextBlock_Loaded;
            tb.DataContextChanged -= TextBlock_DataContextChanged;

            return;
        }


        // =====================================================
        // HINDARI EVENT DUPLIKAT
        // =====================================================

        tb.Loaded -= TextBlock_Loaded;
        tb.Loaded += TextBlock_Loaded;

        tb.DataContextChanged -=
            TextBlock_DataContextChanged;

        tb.DataContextChanged +=
            TextBlock_DataContextChanged;


        // =====================================================
        // JIKA SUDAH MEMILIKI DATACONTEXT
        // LANGSUNG TERAPKAN
        // =====================================================

        ApplyFont(tb);
    }


    // =========================================================
    // LOADED
    // =========================================================

    private static void TextBlock_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not TextBlock tb)
            return;

        ApplyFont(tb);
    }


    // =========================================================
    // DATACONTEXT CHANGED
    // =========================================================
    //
    // BAGIAN INI PENTING UNTUK LISTVIEW VIRTUALIZATION.
    //
    // Ketika ListView melakukan recycling:
    //
    // TextBlock lama
    //      ↓
    // DataContext Ayat A
    //
    // kemudian:
    //
    // TextBlock yang sama
    //      ↓
    // DataContext Ayat B
    //
    // Maka font harus dihitung ulang.
    // =========================================================

    private static void TextBlock_DataContextChanged(
        FrameworkElement sender,
        DataContextChangedEventArgs args)
    {
        if (sender is not TextBlock tb)
            return;

        ApplyFont(tb);
    }


    // =========================================================
    // APPLY FONT
    // =========================================================

    private static void ApplyFont(
        TextBlock tb)
    {
        // =====================================================
        // Pastikan AutoFont masih aktif
        // =====================================================

        if (!GetAutoFont(tb))
            return;


        // =====================================================
        // Pastikan DataContext adalah Ayat
        // =====================================================

        if (tb.DataContext is not Ayat ayat)
            return;


        // =====================================================
        // AMBIL FONT DARI CACHE
        // =====================================================

        if (FontCache.TryGetValue(
                ayat.KodeFont,
                out FontFamily? cachedFont))
        {
            tb.FontFamily =
                cachedFont;

            return;
        }


        // =====================================================
        // AMBIL FONT MANAGER
        // =====================================================

        var manager =
            App.Services.GetService(
                typeof(IFontManager))
            as IFontManager;


        if (manager is null)
            return;


        // =====================================================
        // TENTUKAN FONT
        // =====================================================

        FontFamily font;


        if (ayat.KodeFont == 0)
        {
            // =================================================
            // UNICODE
            // =================================================

            font =
                manager.GetUnicodeFont();
        }
        else
        {
            // =================================================
            // HANDWRITTEN / SYMBOL
            // =================================================

            font =
                manager.GetSymbolFont(
                    ayat.KodeFont);
        }


        // =====================================================
        // SIMPAN KE CACHE
        // =====================================================

        FontCache[
            ayat.KodeFont] =
            font;


        // =====================================================
        // TERAPKAN
        // =====================================================

        tb.FontFamily =
            font;
    }
}