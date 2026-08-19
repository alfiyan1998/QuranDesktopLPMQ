using Microsoft.UI.Xaml.Controls;

namespace QuranKemenag.Views;

/// <summary>
/// Status bar di bagian bawah aplikasi. Di Tahap 2, semua nilai (Mode, Surah,
/// Ayah, Page, Zoom) masih statis/placeholder. Nilai ini akan diperbarui secara
/// dinamis melalui data binding ke ViewModel pada tahap-tahap berikutnya.
/// </summary>
public sealed partial class StatusBarView : UserControl
{
    public StatusBarView()
    {
        this.InitializeComponent();
    }
}
