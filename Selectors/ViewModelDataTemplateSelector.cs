using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QuranKemenag.ViewModels;

namespace QuranKemenag.Selectors;

/// <summary>
/// Simple DataTemplateSelector that maps ViewModel types to DataTemplates defined in Application resources.
/// This keeps the MainWindow XAML clean and centralizes View mapping.
/// </summary>
public sealed class ViewModelDataTemplateSelector : DataTemplateSelector
{
    protected override DataTemplate? SelectTemplateCore(object item)
    {
        if (item is null)
            return null;

        string key = item switch
        {
            HomeViewModel => "HomeViewTemplate",
            SurahViewModel => "SurahViewTemplate",
            JuzViewModel => "JuzViewTemplate",
            PageViewModel => "PageViewTemplate",
            BookmarkViewModel => "BookmarkViewTemplate",
            SettingsViewModel => "SettingsViewTemplate",
            _ => string.Empty
        };

        if (!string.IsNullOrEmpty(key) && Application.Current?.Resources?.ContainsKey(key) == true)
        {
            return Application.Current.Resources[key] as DataTemplate;
        }

        return base.SelectTemplateCore(item);
    }
}
