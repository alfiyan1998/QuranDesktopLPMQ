using System;

namespace QuranKemenag.Constants;

/// <summary>
/// Jenis blok pada halaman Tentang Aplikasi.
/// </summary>
public enum AboutBlockType
{
    /// <summary>Judul bagian: huruf tebal, rata tengah.</summary>
    Heading,

    /// <summary>Paragraf biasa.</summary>
    Paragraph,

    /// <summary>Daftar butir sederhana (satu tingkat).</summary>
    BulletList,

    /// <summary>
    /// Butir bertingkat: satu butir induk (tebal) dengan beberapa
    /// butir anak yang menjorok ke dalam.
    /// </summary>
    BulletGroup,

    /// <summary>
    /// Daftar nama (susunan tim): satu jabatan (tebal) diikuti satu
    /// atau beberapa nama. Rata tengah.
    /// </summary>
    Credit
}

/// <summary>
/// Satu blok isi pada halaman Tentang Aplikasi.
/// Buat dengan salah satu method statis di bawah
/// (Heading, Paragraph, Bullets, BulletGroup, Credit).
/// </summary>
public sealed class AboutBlock
{
    public AboutBlockType Type { get; }

    /// <summary>
    /// Teks utama: isi judul/paragraf, judul butir induk (BulletGroup),
    /// atau nama jabatan (Credit).
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Daftar isi: butir (BulletList), butir anak (BulletGroup),
    /// atau nama-nama (Credit).
    /// </summary>
    public string[] Items { get; }

    private AboutBlock(AboutBlockType type, string text, string[] items)
    {
        Type = type;
        Text = text;
        Items = items;
    }

    public static AboutBlock Heading(string text) =>
        new AboutBlock(AboutBlockType.Heading, text, Array.Empty<string>());

    public static AboutBlock Paragraph(string text) =>
        new AboutBlock(AboutBlockType.Paragraph, text, Array.Empty<string>());

    public static AboutBlock Bullets(params string[] items) =>
        new AboutBlock(AboutBlockType.BulletList, string.Empty, items);

    public static AboutBlock BulletGroup(string title, params string[] items) =>
        new AboutBlock(AboutBlockType.BulletGroup, title, items);

    public static AboutBlock Credit(string role, params string[] names) =>
        new AboutBlock(AboutBlockType.Credit, role, names);
}

/// <summary>
/// ==================================================================
/// ISI HALAMAN "TENTANG APLIKASI"
/// ==================================================================
///
/// INI SATU-SATUNYA FILE YANG PERLU ANDA EDIT untuk mengubah isi
/// dialog Tentang Aplikasi.
///
/// CARA MENULIS ISI
///   Isi disusun sebagai daftar BLOK, dari atas ke bawah, persis seperti
///   dokumen. Ada lima jenis blok:
///
///     AboutBlock.Heading("Judul")
///         Judul bagian: tebal dan rata tengah.
///
///     AboutBlock.Paragraph("Teks ...")
///         Paragraf biasa (rata kiri-kanan).
///
///     AboutBlock.Bullets("a", "b", "c")
///         Daftar butir satu tingkat.
///
///     AboutBlock.BulletGroup("Judul butir", "isi 1", "isi 2")
///         Butir bertingkat: "Judul butir" tebal dengan titik, lalu
///         "isi 1", "isi 2" menjorok ke dalam di bawahnya.
///
///     AboutBlock.Credit("Jabatan", "Nama 1", "Nama 2")
///         Susunan tim: jabatan tebal lalu nama-nama, rata tengah.
///
///   Setiap blok dipisah koma. Urutan di bawah = urutan tampil.
///
/// CARA MENEBALKAN KATA DI DALAM TEKS
///   Apit kata dengan dua tanda bintang:
///
///     "**Qur'an Kemenag Desktop** adalah aplikasi ..."
///
///   Hasilnya: "Qur'an Kemenag Desktop" tampil TEBAL, sisanya biasa.
///   Berlaku di Paragraph, butir Bullets, dan butir anak BulletGroup.
///
/// CATATAN PENULISAN
///   - Tanda kutip ganda (") di dalam teks harus ditulis \" .
///     Tanda petik tunggal (') dan (’) aman.
///   - Teks panjang boleh dipecah ke beberapa baris dengan tanda +.
///     Jangan lupa spasi di akhir potongan, agar kata tidak menyambung.
///
/// PENGATURAN TAMPILAN (rata tengah/kiri-kanan, ukuran huruf)
///   Ada di Views/AboutDialog.xaml.cs, bagian "PENGATURAN TAMPILAN".
/// ==================================================================
/// </summary>
public static class AboutInfo
{
    // -------------------------------------------------------------
    // NAMA APLIKASI
    // Tampil di bagian atas dialog, di samping logo.
    // (Judul "Tentang Aplikasi" sudah menjadi judul dialog.)
    // -------------------------------------------------------------
    public const string AppName =
        "Qur'an Kemenag Desktop";


    // -------------------------------------------------------------
    // ISI (berurutan dari atas ke bawah)
    // Sumber: dokumen "Tentang Aplikasi - Qur'an Kemenag Dekstop.doc"
    // -------------------------------------------------------------
    public static readonly AboutBlock[] Blocks =
    {
        AboutBlock.Paragraph(
            "**Qur'an Kemenag Desktop** adalah aplikasi mushaf Al-Qur'an " +
            "digital berbasis komputer (Windows) yang dikembangkan pada tahun " +
            "**2026** oleh Kementerian Agama Republik Indonesia c.q. Lajnah " +
            "Pentashihan Mushaf Al-Qur’an. Aplikasi ini dirancang untuk " +
            "memudahkan masyarakat, akademisi, pengajar, serta penyusun " +
            "presentasi dalam membaca, mempelajari, dan mengutip ayat-ayat " +
            "Al-Qur'an secara praktis dan fleksibel."),

        AboutBlock.Paragraph(
            "Dengan antarmuka yang modern, bersih, dan intuitif, aplikasi ini " +
            "menyediakan berbagai navigasi membaca lengkap serta penyesuaian " +
            "tampilan yang dapat diatur sesuai kebutuhan pengguna."),

        AboutBlock.Heading("Fitur Utama & Keunggulan"),

        AboutBlock.BulletGroup(
            "Navigasi & Kemudahan Akses Bacaan",
            "**Pilihan Mode Pembacaan:** Memudahkan navigasi berdasarkan " +
            "pilihan **Per Surah**, **Per Juz**, maupun **Per Halaman " +
            "(Mushaf)**.",
            "**Lanjutkan Bacaan & Bookmark:** Fitur penanda otomatis untuk " +
            "melanjutkan bacaan terakhir serta menyimpan ayat-ayat " +
            "favorit/penting.",
            "**Pencarian Cepat:** Fitur pencarian pintar untuk menemukan " +
            "surah, ayat, atau kata kunci tertentu secara instan."),

        AboutBlock.BulletGroup(
            "Fitur Ekspor & Penyalinan Teks (Fitur Tabel/Menu Copy)",
            "Dilengkapi tombol **Copy** di bagian navigasi atas yang berfungsi " +
            "untuk **mengeksport dan menyalin ayat-ayat tertentu** yang " +
            "dipilih oleh pengguna.",
            "Hasil salinan dirancang agar mudah dan rapi saat ditempel (paste) " +
            "langsung ke dokumen **Microsoft Office (Word)** maupun bahan " +
            "presentasi **Microsoft PowerPoint**."),

        AboutBlock.BulletGroup(
            "Kustomisasi & Tampilan Interaktif (Feature Panel)",
            "**Pengaturan Font & Mode:** Pengguna dapat memilih jenis font " +
            "(Computer Font) serta menyesuaikan Mode Tampilan.",
            "**Dukungan Terjemahan & Bahasa Isyarat:** Menyediakan opsi untuk " +
            "menampilkan terjemahan teks serta bahasa isyarat untuk " +
            "aksesibilitas yang lebih luas.",
            "**Penyesuaian Ukuran Teks Fleksibel:** Pengaturan slider mandiri " +
            "untuk menyesuaikan ukuran teks Arab, ukuran teks terjemahan, dan " +
            "ukuran teks bahasa isyarat demi kenyamanan membaca."),

        AboutBlock.Heading("Tim Pengembang"),

        AboutBlock.Credit(
            "Kepala LPMQ (Penanggung Jawab)",
            "Dr. H. Abdul Aziz Sidqi, M.Ag."),

        AboutBlock.Credit(
            "Koordinator Digitalisasi Al-Qur’an dan Produk Kajian",
            "Dr. H. Zarkasi Afif, M.A."),

        AboutBlock.Credit(
            "Ketua Tim",
            "Agus Nurul Saripudin, S.Kom."),

        AboutBlock.Credit(
            "Sekretaris Tim",
            "Alfiyan Nugraha, S.Kom."),

        AboutBlock.Credit(
            "Anggota",
            "Anoraga Widi Okka, S.T.",
            "Asri Asrori, S.T.",
            "Zulfikar Al Farisi, S.Kom.",
            "Febi Tri Hardiyanto, S.Kom.",
            "Muhammad Zamroni Ahbab, S.S.I.",
            "Salim Rusydi Cahyono, Lc.",
            "Muhammad Hilman Hirzi, S.Ag.")

    };


    // -------------------------------------------------------------
    // HAK CIPTA (opsional)
    // Tampil kecil dan samar di bagian paling bawah.
    // Kosongkan ("") jika tidak ingin ditampilkan.
    // -------------------------------------------------------------
    public const string Copyright =
        "";
}