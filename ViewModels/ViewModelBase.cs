// csharp
using CommunityToolkit.Mvvm.ComponentModel;

namespace QuranKemenag.ViewModels;

/// <summary>
/// Base class untuk semua ViewModel di aplikasi.
/// Menggunakan ObservableObject dari CommunityToolkit.Mvvm untuk notifikasi property change.
/// </summary>
public abstract partial class ViewModelBase : ObservableObject
{
}