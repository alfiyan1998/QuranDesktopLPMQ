using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace QuranKemenag.Services.Copy;

public sealed class PowerPointCopyService
{
    // ============================================================
    // WINDOWS COM API
    // ============================================================

    [DllImport(
        "oleaut32.dll",
        CharSet = CharSet.Unicode,
        PreserveSig = false)]
    private static extern void GetActiveObject(
        ref Guid rclsid,
        IntPtr reserved,
        [MarshalAs(UnmanagedType.Interface)]
        out object ppunk);


    // ============================================================
    // COPY TO POWERPOINT
    // ============================================================

    public void CopyToPowerPoint(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            Debug.WriteLine(
                "[PPT COPY] Text kosong.");

            return;
        }

        object? powerPointApp = null;
        object? presentation = null;

        try
        {
            // ====================================================
            // 1. CARI POWERPOINT YANG SUDAH TERBUKA
            // ====================================================

            powerPointApp =
                GetRunningPowerPoint();


            // ====================================================
            // 2. POWERPOINT BELUM TERBUKA
            // ====================================================

            if (powerPointApp == null)
            {
                Debug.WriteLine(
                    "[PPT COPY] PowerPoint belum terbuka.");

                Type? powerPointType =
                    Type.GetTypeFromProgID(
                        "PowerPoint.Application");

                if (powerPointType == null)
                {
                    throw new InvalidOperationException(
                        "Microsoft PowerPoint tidak ditemukan.");
                }

                powerPointApp =
                    Activator.CreateInstance(
                        powerPointType);

                Debug.WriteLine(
                    "[PPT COPY] Membuka PowerPoint baru.");
            }
            else
            {
                Debug.WriteLine(
                    "[PPT COPY] PowerPoint sedang terbuka.");
            }


            // ====================================================
            // 3. VISIBLE
            // ====================================================

            dynamic app =
                powerPointApp;

            app.Visible = true;


            // ====================================================
            // 4. CEK PRESENTATION
            // ====================================================

            dynamic presentations =
                app.Presentations;


            if (presentations.Count > 0)
            {
                // -----------------------------------------------
                // Sudah ada presentation
                // -----------------------------------------------

                presentation =
                    app.ActivePresentation;

                Debug.WriteLine(
                    "[PPT COPY] Menggunakan presentation aktif.");
            }
            else
            {
                // -----------------------------------------------
                // Belum ada presentation
                // -----------------------------------------------

                presentation =
                    presentations.Add();

                Debug.WriteLine(
                    "[PPT COPY] Membuat presentation baru.");
            }


            dynamic activePresentation =
                presentation;


            // ====================================================
            // 5. AMBIL SLIDE AKTIF
            // ====================================================

            dynamic? slide = null;


            try
            {
                // ------------------------------------------------
                // Jika ada window aktif dan slide aktif,
                // gunakan slide tersebut.
                // ------------------------------------------------

                if (app.Windows.Count > 0)
                {
                    dynamic window =
                        app.ActiveWindow;

                    if (window != null)
                    {
                        try
                        {
                            slide =
                                window.View.Slide;
                        }
                        catch
                        {
                            slide = null;
                        }
                    }
                }
            }
            catch
            {
                slide = null;
            }


            // ====================================================
            // 6. JIKA TIDAK ADA SLIDE AKTIF
            // ====================================================

            if (slide == null)
            {
                dynamic slides =
                    activePresentation.Slides;


                if (slides.Count == 0)
                {
                    // --------------------------------------------
                    // Presentation benar-benar kosong.
                    // Buat slide pertama.
                    // --------------------------------------------

                    slide =
                        slides.Add(
                            1,
                            12);

                    Debug.WriteLine(
                        "[PPT COPY] Membuat slide pertama.");
                }
                else
                {
                    // --------------------------------------------
                    // Presentation punya slide,
                    // tetapi tidak ada slide aktif.
                    //
                    // Gunakan slide pertama.
                    // --------------------------------------------

                    slide =
                        slides[1];

                    Debug.WriteLine(
                        "[PPT COPY] Menggunakan slide pertama.");
                }
            }
            else
            {
                Debug.WriteLine(
                    "[PPT COPY] Menggunakan slide yang sedang aktif.");
            }


            // ====================================================
            // 7. CEK SHAPE YANG SEDANG DIPILIH
            // ====================================================

            dynamic? selectedShape = null;

            try
            {
                if (app.ActiveWindow != null)
                {
                    dynamic selection =
                        app.ActiveWindow.Selection;

                    if (selection != null)
                    {
                        try
                        {
                            if (selection.Type == 14 &&
                                selection.ShapeRange.Count > 0)
                            {
                                selectedShape =
                                    selection.ShapeRange[1];
                            }
                        }
                        catch
                        {
                            selectedShape = null;
                        }
                    }
                }
            }
            catch
            {
                selectedShape = null;
            }


            // ====================================================
            // 8. JIKA ADA TEXTBOX YANG DIPILIH
            // ====================================================

            if (selectedShape != null)
            {
                try
                {
                    dynamic textFrame =
                        selectedShape.TextFrame;

                    if (textFrame.HasTextFrame == -1)
                    {
                        textFrame.TextRange.Text =
                            text;

                        textFrame.TextRange.Font.Size =
                            24;

                        Debug.WriteLine(
                            "[PPT COPY] Teks dimasukkan ke shape yang dipilih.");

                        return;
                    }
                }
                catch
                {
                    // Shape bukan textbox.
                    // Lanjut membuat textbox pada slide aktif.
                }
            }


            // ====================================================
            // 9. BUAT TEXTBOX PADA SLIDE AKTIF
            // ====================================================

            dynamic shapes =
                slide.Shapes;


            dynamic textBox =
                shapes.AddTextbox(
                    1,
                    30,
                    30,
                    650,
                    450);


            // ====================================================
            // 10. MASUKKAN TEXT
            // ====================================================

            dynamic textFrameNew =
                textBox.TextFrame;


            textFrameNew.TextRange.Text =
                text;


            textFrameNew.TextRange.Font.Size =
                24;


            // ====================================================
            // 11. DEBUG
            // ====================================================

            Debug.WriteLine(
                "[PPT COPY] Text berhasil dimasukkan ke slide aktif.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                "[PPT COPY] ERROR:");

            Debug.WriteLine(ex);
        }
        finally
        {
            powerPointApp = null;
            presentation = null;
        }
    }


    // ============================================================
    // GET RUNNING POWERPOINT
    // ============================================================

    private static object? GetRunningPowerPoint()
    {
        try
        {
            Guid powerPointGuid =
                new Guid(
                    "91493441-5A91-11CF-8700-00AA0060263B");

            GetActiveObject(
                ref powerPointGuid,
                IntPtr.Zero,
                out object powerPointObject);

            return powerPointObject;
        }
        catch
        {
            return null;
        }
    }
}