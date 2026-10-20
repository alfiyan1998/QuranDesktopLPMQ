using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using QuranKemenag.Models;
using QuranKemenag.Repositories;

namespace QuranKemenag.Services.Text;

/// <summary>
/// Penyedia Bismillah versi TULISAN TANGAN (Handwritten Manuscript)
/// untuk banner di atas ayat pertama setiap surah.
///
/// KENAPA ADA FILE INI?
///   Banner Bismillah lama berupa TextBlock Unicode berfont
///   "LPMQ Isep Misbah", sehingga pada mode Handwritten bentuknya
///   tidak sama dengan ayat di bawahnya.
///
/// DARI MANA DATANYA?
///   Bismillah adalah ayat 1 Al-Fatihah. Di database simbol
///   (msi_simbol.db) ayat itu sudah tersedia dalam bentuk glyph
///   tulisan tangan. Jadi cukup mengambil Al-Fatihah ayat 1 lewat
///   jalur yang sama dengan yang dipakai Reader.
///
/// KENAPA DISIMPAN (CACHE)?
///   Bismillah selalu sama untuk semua surah. Dengan menyimpan hasilnya,
///   database hanya dibaca satu kali, bukan setiap kartu dibuat.
/// </summary>
public static class HandwrittenBismillahProvider
{
    // Tugas pemuatan yang sedang berjalan atau sudah selesai.
    // Dipakai bersama agar kartu-kartu yang meminta hampir bersamaan
    // tidak memuat data yang sama berkali-kali.
    private static Task<Ayat?>? _loadTask;

    /// <summary>
    /// Mengembalikan Ayat Bismillah tulisan tangan, atau null jika
    /// datanya tidak tersedia (pemanggil harus menyiapkan cadangan,
    /// misalnya menampilkan teks Unicode).
    /// </summary>
    public static async Task<Ayat?> GetAsync()
    {
        // Mulai memuat jika belum pernah, atau pernah gagal.
        _loadTask ??= LoadAsync();

        Ayat? result = await _loadTask;

        // Gagal (null): kosongkan agar pemanggilan berikutnya mencoba lagi.
        if (result is null)
            _loadTask = null;

        return result;
    }

    private static async Task<Ayat?> LoadAsync()
    {
        try
        {
            var services = App.Services;

            var repository =
                services?.GetService<ISymbolRepository>();

            var processor =
                services?.GetService<QuranKemenag.Services.Text.ISymbolTextProcessor>();

            if (repository is null || processor is null)
                return null;

            // Al-Fatihah (surah 1) ayat 1 = Bismillah.
            var symbol = await repository.GetAyatAsync(1, 1);

            if (symbol is null)
                return null;

            // Processor menyusun urutan karakter sama seperti Reader.
            var ayat =
                processor.Process(new[] { symbol }).FirstOrDefault();

            if (ayat is null ||
                ayat.KodeFont <= 0 ||
                string.IsNullOrWhiteSpace(ayat.Teks))
            {
                return null;
            }

            // Token pertama pada Teks adalah glyph LINGKARAN NOMOR AYAT.
            // Pada banner Bismillah, lingkaran itu tidak boleh ikut tampil,
            // jadi dibuang dengan aturan yang sama seperti
            // AyatCard.RenderHandwritten.
            string raw = ayat.Teks.Trim();

            int firstSpace =
                raw.IndexOfAny(new[] { ' ', '\u00A0' });

            if (firstSpace == 1)
                raw = raw.Substring(firstSpace + 1).TrimStart();

            return new Ayat
            {
                KodeFont = ayat.KodeFont,
                Teks = raw
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[BISMILLAH HANDWRITTEN] Gagal memuat:");
            Debug.WriteLine(ex);

            return null;
        }
    }
}