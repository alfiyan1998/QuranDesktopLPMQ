using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace QuranKemenag.Views;

public sealed partial class TestFontPage : Page
{
    // Keep owner field to avoid incremental compilation issues when hot-reload is active.
    // The field is intentionally unused; TestFontPage is inert for now.
#pragma warning disable CS0169 // Field sengaja tidak dipakai (lihat komentar di atas)
    private Window? _ownerWindow;
#pragma warning restore CS0169
    public TestFontPage()
    {
        this.InitializeComponent();
    }
}
