using System;
using System.Diagnostics;
using System.Threading.Tasks;
using QuranKemenag.Repositories;

namespace QuranKemenag.Services;

/// <summary>
/// Test sederhana untuk memastikan repository dapat
/// mengambil posisi awal setiap Juz dari database Quran Kemenag.
/// </summary>
public static class JuzRepositoryDebugTest
{
    public static async Task RunAsync(
        IQuranRepository repository)
    {
        Debug.WriteLine("========================================");
        Debug.WriteLine("JUZ REPOSITORY TEST");
        Debug.WriteLine("========================================");

        try
        {
            for (int juzNumber = 1; juzNumber <= 30; juzNumber++)
            {
                var result =
                    await repository.GetJuzStartAsync(juzNumber);

                if (result is null)
                {
                    Debug.WriteLine(
                        $"Juz {juzNumber} : TIDAK ADA DATA");

                    continue;
                }

                Debug.WriteLine(
                    $"Juz {result.Juz} : " +
                    $"Surah={result.Surah}, " +
                    $"Ayat={result.Ayat}, " +
                    $"Halaman={result.Halaman}");
            }

            Debug.WriteLine("----------------------------------------");
            Debug.WriteLine("JUZ REPOSITORY TEST SELESAI");
            Debug.WriteLine("========================================");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("----------------------------------------");
            Debug.WriteLine("JUZ REPOSITORY TEST ERROR");
            Debug.WriteLine("----------------------------------------");

            Debug.WriteLine(ex.ToString());

            Debug.WriteLine("========================================");
        }
    }
}