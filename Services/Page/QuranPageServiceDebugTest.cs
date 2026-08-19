using System.Diagnostics;
using QuranKemenag.Models;

namespace QuranKemenag.Services.Page;

/// <summary>
/// Test sementara untuk memverifikasi QuranPageService.
/// File ini hanya digunakan selama pengembangan
/// dan akan dihapus setelah verifikasi selesai.
/// </summary>
internal static class QuranPageServiceDebugTest
{
    public static void Run()
    {
        Debug.WriteLine("");
        Debug.WriteLine("========================================");
        Debug.WriteLine("QURAN PAGE SERVICE TEST");
        Debug.WriteLine("========================================");


        // =====================================================
        // TEST DATA
        // =====================================================

        var ayats = new List<Ayat>
        {
            new()
            {
                Id = 1,
                Surah = 1,
                AyatNumber = 1,
                Juz = 1,
                Halaman = 1,
                Teks = "Ayat 1"
            },

            new()
            {
                Id = 2,
                Surah = 1,
                AyatNumber = 2,
                Juz = 1,
                Halaman = 1,
                Teks = "Ayat 2"
            },

            new()
            {
                Id = 3,
                Surah = 1,
                AyatNumber = 3,
                Juz = 1,
                Halaman = 1,
                Teks = "Ayat 3"
            },

            new()
            {
                Id = 4,
                Surah = 2,
                AyatNumber = 1,
                Juz = 1,
                Halaman = 2,
                Teks = "Al-Baqarah Ayat 1"
            },

            new()
            {
                Id = 5,
                Surah = 2,
                AyatNumber = 2,
                Juz = 1,
                Halaman = 2,
                Teks = "Al-Baqarah Ayat 2"
            },

            new()
            {
                Id = 6,
                Surah = 2,
                AyatNumber = 3,
                Juz = 1,
                Halaman = 2,
                Teks = "Al-Baqarah Ayat 3"
            },

            // Simulasikan database lama:
            // belum memiliki halaman.
            new()
            {
                Id = 7,
                Surah = 2,
                AyatNumber = 4,
                Juz = 1,
                Halaman = null,
                Teks = "Ayat tanpa halaman"
            }
        };


        // =====================================================
        // CREATE SERVICE
        // =====================================================

        IQuranPageService service =
            new QuranPageService();


        // =====================================================
        // BUILD PAGES
        // =====================================================

        var pages =
            service.BuildPages(ayats);


        Debug.WriteLine(
            $"Jumlah halaman : {pages.Count}");


        // =====================================================
        // TEST JUMLAH HALAMAN
        // =====================================================

        if (pages.Count != 2)
        {
            throw new Exception(
                $"TEST GAGAL: jumlah halaman seharusnya 2, " +
                $"tetapi hasil = {pages.Count}");
        }


        // =====================================================
        // TEST PAGE 1
        // =====================================================

        var page1 =
            service.GetPage(
                pages,
                1);

        if (page1 is null)
        {
            throw new Exception(
                "TEST GAGAL: Page 1 tidak ditemukan.");
        }


        if (page1.Ayats.Count != 3)
        {
            throw new Exception(
                $"TEST GAGAL: Page 1 seharusnya memiliki 3 ayat, " +
                $"tetapi hasil = {page1.Ayats.Count}");
        }


        // =====================================================
        // TEST URUTAN PAGE 1
        // =====================================================

        if (page1.Ayats[0].AyatNumber != 1 ||
            page1.Ayats[1].AyatNumber != 2 ||
            page1.Ayats[2].AyatNumber != 3)
        {
            throw new Exception(
                "TEST GAGAL: Urutan ayat Page 1 salah.");
        }


        // =====================================================
        // TEST PAGE 2
        // =====================================================

        var page2 =
            service.GetPage(
                pages,
                2);

        if (page2 is null)
        {
            throw new Exception(
                "TEST GAGAL: Page 2 tidak ditemukan.");
        }


        if (page2.Ayats.Count != 3)
        {
            throw new Exception(
                $"TEST GAGAL: Page 2 seharusnya memiliki 3 ayat, " +
                $"tetapi hasil = {page2.Ayats.Count}");
        }


        // =====================================================
        // TEST AYAT PAGE 2
        // =====================================================

        if (page2.Ayats[0].Surah != 2 ||
            page2.Ayats[0].AyatNumber != 1)
        {
            throw new Exception(
                "TEST GAGAL: Ayat pertama Page 2 salah.");
        }


        // =====================================================
        // TEST AYAT TANPA HALAMAN
        // =====================================================

        foreach (var page in pages)
        {
            foreach (var ayat in page.Ayats)
            {
                if (!ayat.Halaman.HasValue)
                {
                    throw new Exception(
                        "TEST GAGAL: Ayat tanpa halaman masuk ke Page.");
                }
            }
        }


        // =====================================================
        // SUCCESS
        // =====================================================

        Debug.WriteLine("");
        Debug.WriteLine("TEST BERHASIL");
        Debug.WriteLine("");
        Debug.WriteLine(
            $"Page 1 : {page1.Ayats.Count} ayat");

        Debug.WriteLine(
            $"Page 2 : {page2.Ayats.Count} ayat");

        Debug.WriteLine(
            "Ayat tanpa halaman berhasil diabaikan.");

        Debug.WriteLine("========================================");
        Debug.WriteLine("");
    }
}