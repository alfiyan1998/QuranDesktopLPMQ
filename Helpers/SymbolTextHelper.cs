using System;

namespace QuranKemenag.Helpers;

/// <summary>
/// Utility helper untuk penanganan teks dari Database Symbol.
/// <para>
/// Kelas ini bersifat statis dan menyediakan fungsi ringan yang hanya
/// membalik urutan kata dalam sebuah string. Dirancang sebagai solusi
/// sementara yang mudah dihapus ketika database Symbol sudah menggunakan
/// urutan kata yang benar.
/// </para>
/// </summary>
/// <remarks>
/// Semua komentar dan dokumentasi ditulis dalam Bahasa Indonesia.
/// Implementasi dibuat sekecil mungkin agar mudah diuji dan aman dipanggil
/// dari layer yang tepat (mis. mapping/adapter pada lapisan data atau
/// presenter/service sebelum memasukkan data ke ViewModel).
/// </remarks>
public static class SymbolTextHelper
{
    // -----------------------------------------------------------------
    // Catatan umum:
    // - Metode ini tidak melakukan trimming kecuali diperlukan oleh Split.
    // - Menggunakan StringSplitOptions.RemoveEmptyEntries seperti permintaan.
    // - Mengembalikan string kosong apabila input null atau kosong.
    // -----------------------------------------------------------------

    /// <summary>
    /// Membalik urutan kata pada sebuah teks.
    /// </summary>
    /// <param name="input">Teks input yang berasal dari Database Symbol.</param>
    /// <returns>
    /// Teks dengan urutan kata dibalik. Bila <paramref name="input"/> bernilai
    /// <see langword="null"/> atau string kosong, maka mengembalikan string kosong.
    /// </returns>
    /// <example>
    /// Contoh penggunaan:
    /// <code>
    /// var s = "الرَّحِيمِ الرَّحْمَٰنِ اللَّهِ بِسْمِ";
    /// var out = SymbolTextHelper.ReverseWords(s);
    /// // hasil: "بِسْمِ اللَّهِ الرَّحْمَٰنِ الرَّحِيمِ"
    /// </code>
    /// </example>
    /// <remarks>
    /// Implementasi dibuat sesuai persyaratan:
    /// - Mengembalikan string.Empty jika input null/empty.
    /// - Menggunakan <see cref="String.Split(char[], StringSplitOptions)"/> dengan <see cref="StringSplitOptions.RemoveEmptyEntries"/>.
    /// - Membalik urutan elemen menggunakan <see cref="Array.Reverse(Array)"/>.
    /// - Menggabungkan kembali menggunakan <see cref="string.Join(string, string[])"/> dengan separator spasi.
    /// </remarks>
    public static string ReverseWords(string? input)
    {
        // Kembalikan string kosong jika input null atau kosong
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        // Pisahkan kata-kata, hilangkan entri kosong
        var parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        // Jika hanya satu kata atau tidak ada, tinggal kembalikan aslinya (atau gabungan yang sama)
        if (parts.Length <= 1)
        {
            return string.Join(" ", parts);
        }

        // Balik urutan dan gabungkan kembali
        Array.Reverse(parts);
        var result = string.Join(" ", parts);

        // Debug: tampilkan input dan output agar bisa melacak apakah helper pernah dipanggil
        try
        {
            System.Diagnostics.Debug.WriteLine($"[Helper] ReverseWords input='" + input + "' output='" + result + "'");
        }
        catch
        {
            // ignore
        }

        return result;
    }

    /// <summary>
    /// Membalik urutan karakter dalam sebuah string.
    /// </summary>
    /// <param name="input">Teks input yang akan dibalik karakter per karakternya.</param>
    /// <returns>String baru dengan urutan karakter terbalik. Jika <paramref name="input"/> bernilai <see langword="null"/> atau kosong,
    /// mengembalikan string kosong.</returns>
    /// <remarks>
    /// Implementasi sederhana: konversi ke array <see cref="char"/>, panggil <see cref="Array.Reverse(char[])"/>
    /// dan bangun string baru. Transformasi ini tidak melakukan normalisasi atau pemrosesan BiDi.
    /// Gunakan metode ini hanya sekali, tepat setelah membaca dari database dan sebelum memasukkan ke koleksi.
    /// </remarks>
    public static string ReverseCharacters(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var chars = input.ToCharArray();
        Array.Reverse(chars);
        var result = new string(chars);

        try
        {
            System.Diagnostics.Debug.WriteLine($"[Helper] ReverseCharacters input='" + input + "' output='" + result + "'");
        }
        catch { }

        return result;
    }
}
