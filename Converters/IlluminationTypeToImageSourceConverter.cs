using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;

namespace QuranKemenag.Converters
{
    /// <summary>
    /// Mengubah nilai "AlFatiha" / "AlBaqara" / "Frame" (lihat
    /// MainViewModel.CurrentPageIlluminationType) menjadi asset
    /// PNG iluminasi Mushaf yang sesuai di Assets/Images/Hafs.
    ///
    /// STEP 25.12: hanya menggunakan asset PNG asli, tidak
    /// membuat ornamen manual (Shape/Path/Border).
    /// </summary>
    public sealed class IlluminationTypeToImageSourceConverter : IValueConverter
    {
        private const string BasePath = "ms-appx:///Assets/Images/Hafs/";

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Hafs_Al-Baqara.png masih berupa desain rosette
            // (medalion lingkaran) yang belum ditangani secara
            // khusus — dikerjakan satu-satu sesuai permintaan,
            // giliran berikutnya setelah Al-Fatihah. Untuk sementara
            // tetap memakai Hafs-Frame.png (aman menampung teks).
            var fileName = (value as string) switch
            {
                "AlFatiha" => "Hafs_Al-Fatiha.png",
                _ => "Hafs-Frame.png"
            };

            try
            {
                return new BitmapImage(new Uri(BasePath + fileName));
            }
            catch
            {
                // Asset belum tersedia di project — biarkan Image
                // control kosong daripada membuat aplikasi crash.
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
