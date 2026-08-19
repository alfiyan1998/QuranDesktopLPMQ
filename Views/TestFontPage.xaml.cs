using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace QuranKemenag.Views;

public sealed partial class TestFontPage : Page
{
    // Keep owner field to avoid incremental compilation issues when hot-reload is active.
    // The field is intentionally unused; TestFontPage is inert for now.
    private Window? _ownerWindow;
    public TestFontPage()
    {
        this.InitializeComponent();
    }
}
