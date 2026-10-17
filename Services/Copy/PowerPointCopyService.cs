
//sebelum edit untuk menyalin hasil Copy ke Microsoft PowerPoint dengan format lengkap (font, ukuran, arah baca, perataan) seperti pada Word\

//using System;
//using System.Diagnostics;
//using System.Runtime.InteropServices;

//namespace QuranKemenag.Services.Copy;

//public sealed class PowerPointCopyService
//{
//    // ============================================================
//    // WINDOWS COM API
//    // ============================================================

//    [DllImport(
//        "oleaut32.dll",
//        CharSet = CharSet.Unicode,
//        PreserveSig = false)]
//    private static extern void GetActiveObject(
//        ref Guid rclsid,
//        IntPtr reserved,
//        [MarshalAs(UnmanagedType.Interface)]
//        out object ppunk);


//    // ============================================================
//    // COPY TO POWERPOINT
//    // ============================================================

//    public void CopyToPowerPoint(string text)
//    {
//        if (string.IsNullOrWhiteSpace(text))
//        {
//            Debug.WriteLine(
//                "[PPT COPY] Text kosong.");

//            return;
//        }

//        object? powerPointApp = null;
//        object? presentation = null;

//        try
//        {
//            // ====================================================
//            // 1. CARI POWERPOINT YANG SUDAH TERBUKA
//            // ====================================================

//            powerPointApp =
//                GetRunningPowerPoint();


//            // ====================================================
//            // 2. POWERPOINT BELUM TERBUKA
//            // ====================================================

//            if (powerPointApp == null)
//            {
//                Debug.WriteLine(
//                    "[PPT COPY] PowerPoint belum terbuka.");

//                Type? powerPointType =
//                    Type.GetTypeFromProgID(
//                        "PowerPoint.Application");

//                if (powerPointType == null)
//                {
//                    throw new InvalidOperationException(
//                        "Microsoft PowerPoint tidak ditemukan.");
//                }

//                powerPointApp =
//                    Activator.CreateInstance(
//                        powerPointType);

//                Debug.WriteLine(
//                    "[PPT COPY] Membuka PowerPoint baru.");
//            }
//            else
//            {
//                Debug.WriteLine(
//                    "[PPT COPY] PowerPoint sedang terbuka.");
//            }


//            // ====================================================
//            // 3. VISIBLE
//            // ====================================================

//            dynamic app =
//                powerPointApp;

//            app.Visible = true;


//            // ====================================================
//            // 4. CEK PRESENTATION
//            // ====================================================

//            dynamic presentations =
//                app.Presentations;


//            if (presentations.Count > 0)
//            {
//                // -----------------------------------------------
//                // Sudah ada presentation
//                // -----------------------------------------------

//                presentation =
//                    app.ActivePresentation;

//                Debug.WriteLine(
//                    "[PPT COPY] Menggunakan presentation aktif.");
//            }
//            else
//            {
//                // -----------------------------------------------
//                // Belum ada presentation
//                // -----------------------------------------------

//                presentation =
//                    presentations.Add();

//                Debug.WriteLine(
//                    "[PPT COPY] Membuat presentation baru.");
//            }


//            dynamic activePresentation =
//                presentation;


//            // ====================================================
//            // 5. AMBIL SLIDE AKTIF
//            // ====================================================

//            dynamic? slide = null;


//            try
//            {
//                // ------------------------------------------------
//                // Jika ada window aktif dan slide aktif,
//                // gunakan slide tersebut.
//                // ------------------------------------------------

//                if (app.Windows.Count > 0)
//                {
//                    dynamic window =
//                        app.ActiveWindow;

//                    if (window != null)
//                    {
//                        try
//                        {
//                            slide =
//                                window.View.Slide;
//                        }
//                        catch
//                        {
//                            slide = null;
//                        }
//                    }
//                }
//            }
//            catch
//            {
//                slide = null;
//            }


//            // ====================================================
//            // 6. JIKA TIDAK ADA SLIDE AKTIF
//            // ====================================================

//            if (slide == null)
//            {
//                dynamic slides =
//                    activePresentation.Slides;


//                if (slides.Count == 0)
//                {
//                    // --------------------------------------------
//                    // Presentation benar-benar kosong.
//                    // Buat slide pertama.
//                    // --------------------------------------------

//                    slide =
//                        slides.Add(
//                            1,
//                            12);

//                    Debug.WriteLine(
//                        "[PPT COPY] Membuat slide pertama.");
//                }
//                else
//                {
//                    // --------------------------------------------
//                    // Presentation punya slide,
//                    // tetapi tidak ada slide aktif.
//                    //
//                    // Gunakan slide pertama.
//                    // --------------------------------------------

//                    slide =
//                        slides[1];

//                    Debug.WriteLine(
//                        "[PPT COPY] Menggunakan slide pertama.");
//                }
//            }
//            else
//            {
//                Debug.WriteLine(
//                    "[PPT COPY] Menggunakan slide yang sedang aktif.");
//            }


//            // ====================================================
//            // 7. CEK SHAPE YANG SEDANG DIPILIH
//            // ====================================================

//            dynamic? selectedShape = null;

//            try
//            {
//                if (app.ActiveWindow != null)
//                {
//                    dynamic selection =
//                        app.ActiveWindow.Selection;

//                    if (selection != null)
//                    {
//                        try
//                        {
//                            if (selection.Type == 14 &&
//                                selection.ShapeRange.Count > 0)
//                            {
//                                selectedShape =
//                                    selection.ShapeRange[1];
//                            }
//                        }
//                        catch
//                        {
//                            selectedShape = null;
//                        }
//                    }
//                }
//            }
//            catch
//            {
//                selectedShape = null;
//            }


//            // ====================================================
//            // 8. JIKA ADA TEXTBOX YANG DIPILIH
//            // ====================================================

//            if (selectedShape != null)
//            {
//                try
//                {
//                    dynamic textFrame =
//                        selectedShape.TextFrame;

//                    if (textFrame.HasTextFrame == -1)
//                    {
//                        textFrame.TextRange.Text =
//                            text;

//                        textFrame.TextRange.Font.Size =
//                            24;

//                        Debug.WriteLine(
//                            "[PPT COPY] Teks dimasukkan ke shape yang dipilih.");

//                        return;
//                    }
//                }
//                catch
//                {
//                    // Shape bukan textbox.
//                    // Lanjut membuat textbox pada slide aktif.
//                }
//            }


//            // ====================================================
//            // 9. BUAT TEXTBOX PADA SLIDE AKTIF
//            // ====================================================

//            dynamic shapes =
//                slide.Shapes;


//            dynamic textBox =
//                shapes.AddTextbox(
//                    1,
//                    30,
//                    30,
//                    650,
//                    450);


//            // ====================================================
//            // 10. MASUKKAN TEXT
//            // ====================================================

//            dynamic textFrameNew =
//                textBox.TextFrame;


//            textFrameNew.TextRange.Text =
//                text;


//            textFrameNew.TextRange.Font.Size =
//                24;


//            // ====================================================
//            // 11. DEBUG
//            // ====================================================

//            Debug.WriteLine(
//                "[PPT COPY] Text berhasil dimasukkan ke slide aktif.");
//        }
//        catch (Exception ex)
//        {
//            Debug.WriteLine(
//                "[PPT COPY] ERROR:");

//            Debug.WriteLine(ex);
//        }
//        finally
//        {
//            powerPointApp = null;
//            presentation = null;
//        }
//    }


//    // ============================================================
//    // GET RUNNING POWERPOINT
//    // ============================================================

//    private static object? GetRunningPowerPoint()
//    {
//        try
//        {
//            Guid powerPointGuid =
//                new Guid(
//                    "91493441-5A91-11CF-8700-00AA0060263B");

//            GetActiveObject(
//                ref powerPointGuid,
//                IntPtr.Zero,
//                out object powerPointObject);

//            return powerPointObject;
//        }
//        catch
//        {
//            return null;
//        }
//    }
//}


//sesudah edit untuk menyalin hasil Copy ke Microsoft PowerPoint dengan format lengkap (font, ukuran, arah baca, perataan) seperti pada Word
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using QuranKemenag.Models.Copy;

namespace QuranKemenag.Services.Copy;

/// <summary>
/// Mengirim hasil Copy ke Microsoft PowerPoint.
///
/// ALUR (disamakan dengan WordCopyService):
///   1. Cari PowerPoint yang sedang terbuka (atau buka yang baru).
///   2. Tentukan slide tujuan (lihat PrepareTargetSlide).
///   3. Buat satu kotak teks selebar slide.
///   4. Isi kotak teks per BLOK (Arab, terjemahan, isyarat, dst.),
///      dan setiap blok diberi font, ukuran, arah baca, dan rata
///      paragraf sendiri-sendiri.
///
/// CATATAN PENTING:
///   Font tulisan tangan (Lpmq_Hafs_XX) dan font isyarat harus
///   terpasang di komputer, sama seperti saat memakai Word.
///   PowerPoint hanya memakai nama font; ia tidak membawa file font.
/// </summary>
public sealed class PowerPointCopyService
{
    // ============================================================
    // KONSTANTA
    // ============================================================

    /// <summary>
    /// Ukuran font di CopyBlock dibuat untuk dokumen Word (Arab 18,
    /// terjemahan 11). Di slide huruf harus jauh lebih besar agar
    /// terbaca dari jauh, jadi semua ukuran dikalikan angka ini.
    /// Contoh: Arab 18 -> 32, terjemahan 11 -> 20.
    /// </summary>
    private const double SlideFontScale = 1.8;

    /// <summary>Jarak kotak teks dari tepi slide (satuan point).</summary>
    private const float SlideMargin = 40f;

    /// <summary>
    /// Formatter menukar kurung ornamen untuk tulisan tangan agar benar
    /// di Word (﴾ teks ﴿). PowerPoint menata kurung di ujung paragraf
    /// dengan cara berbeda, sehingga hasilnya terbalik. Jika true,
    /// kurung ditukar kembali khusus untuk PowerPoint.
    /// Ubah ke false bila di PowerPoint Anda kurungnya justru jadi salah.
    /// </summary>
    private const bool FlipBracketsForHandwritten = true;

    // Nilai konstanta bawaan PowerPoint (enum COM), ditulis sebagai angka
    // karena kita memakai "dynamic" tanpa referensi Interop.
    private const int PpLayoutBlank = 12;          // layout slide kosong
    private const int PpDirectionLeftToRight = 1;  // arah baca kiri -> kanan
    private const int PpDirectionRightToLeft = 2;  // arah baca kanan -> kiri
    private const int PpAlignLeft = 1;
    private const int PpAlignCenter = 2;
    private const int PpAlignRight = 3;
    private const int MsoTrue = -1;
    private const int MsoFalse = 0;
    private const int MsoAutoSizeTextToFitShape = 2; // teks mengecil otomatis
    private const int MsoShapeTypeSelection = 14;    // Selection.Type = shape

    // ============================================================
    // WINDOWS COM API
    // ============================================================

    /// <summary>
    /// Mengambil objek PowerPoint yang SUDAH berjalan.
    /// </summary>
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
    // COPY TO POWERPOINT (DATA TERSTRUKTUR)  <-- DIPAKAI DIALOG COPY
    // ============================================================

    /// <summary>
    /// Memasukkan hasil Copy ke PowerPoint dengan format lengkap
    /// (font, ukuran, arah baca, perataan) seperti pada Word.
    /// </summary>
    public void CopyToPowerPoint(CopyResult result)
    {
        if (result == null || result.IsEmpty)
        {
            Debug.WriteLine("[PPT COPY] CopyResult kosong.");
            return;
        }

        try
        {
            // 1. Ambil / buka PowerPoint.
            dynamic app = GetOrCreatePowerPoint();

            // 2. Ambil / buat presentation.
            dynamic presentation = GetOrCreatePresentation(app);

            // 3. Jika user sedang memilih sebuah kotak teks,
            //    isi kotak itu (perilaku lama dipertahankan).
            dynamic? selectedShape = GetSelectedTextShape(app);

            if (selectedShape != null)
            {
                FillTextFrame(selectedShape.TextFrame, result);

                Debug.WriteLine(
                    "[PPT COPY] Hasil dimasukkan ke shape yang dipilih.");

                return;
            }

            // 4. Tentukan slide tujuan (slide kosong atau slide baru).
            dynamic slide = PrepareTargetSlide(app, presentation);

            // 5. Buat kotak teks selebar slide.
            dynamic textBox = AddFullWidthTextBox(presentation, slide);

            // 6. Isi kotak teks blok demi blok.
            FillTextFrame(textBox.TextFrame, result);

            // 7. Teks yang terlalu panjang mengecil otomatis
            //    agar tidak keluar dari slide.
            EnableShrinkOnOverflow(textBox);

            Debug.WriteLine(
                "[PPT COPY] CopyResult berhasil dimasukkan ke slide.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[PPT COPY] ERROR:");
            Debug.WriteLine(ex);
        }
    }


    // ============================================================
    // COPY TO POWERPOINT (TEKS BIASA)  <-- VERSI LAMA, DIPERTAHANKAN
    // ============================================================

    /// <summary>
    /// Versi lama: hanya menempel teks mentah tanpa format font.
    /// Tidak dipakai lagi oleh dialog Copy karena tulisan tangan
    /// akan tampil berantakan. Gunakan overload CopyResult.
    /// </summary>
    public void CopyToPowerPoint(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        // Dibungkus menjadi satu blok Terjemahan (teks Latin biasa)
        // lalu memakai jalur yang sama dengan versi terstruktur.
        var result = new CopyResult();

        result.Blocks.Add(
            new CopyBlock
            {
                Text = text,
                Type = CopyBlockType.Translation,
                FontSize = 11,
                IsRightToLeft = false
            });

        CopyToPowerPoint(result);
    }


    // ============================================================
    // 1. POWERPOINT APPLICATION
    // ============================================================

    /// <summary>
    /// Mengembalikan PowerPoint yang sedang terbuka. Jika belum ada,
    /// PowerPoint baru dibuka. Lalu dipastikan terlihat di layar.
    /// </summary>
    private static dynamic GetOrCreatePowerPoint()
    {
        object? powerPointApp = GetRunningPowerPoint();

        if (powerPointApp == null)
        {
            Type? powerPointType =
                Type.GetTypeFromProgID("PowerPoint.Application");

            if (powerPointType == null)
            {
                throw new InvalidOperationException(
                    "Microsoft PowerPoint tidak ditemukan.");
            }

            powerPointApp = Activator.CreateInstance(powerPointType);

            Debug.WriteLine("[PPT COPY] Membuka PowerPoint baru.");
        }
        else
        {
            Debug.WriteLine("[PPT COPY] PowerPoint sedang terbuka.");
        }

        dynamic app = powerPointApp!;

        app.Visible = true;

        return app;
    }

    /// <summary>
    /// Mengambil objek PowerPoint yang sudah berjalan,
    /// atau null jika PowerPoint belum dibuka.
    /// </summary>
    private static object? GetRunningPowerPoint()
    {
        try
        {
            Guid powerPointGuid =
                new Guid("91493441-5A91-11CF-8700-00AA0060263B");

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


    // ============================================================
    // 2. PRESENTATION
    // ============================================================

    /// <summary>
    /// Memakai presentation yang sedang aktif,
    /// atau membuat yang baru jika belum ada.
    /// </summary>
    private static dynamic GetOrCreatePresentation(dynamic app)
    {
        dynamic presentations = app.Presentations;

        if (presentations.Count > 0)
            return app.ActivePresentation;

        return presentations.Add();
    }


    // ============================================================
    // 3. SHAPE YANG SEDANG DIPILIH USER
    // ============================================================

    /// <summary>
    /// Jika user sedang memilih sebuah kotak teks di slide,
    /// kembalikan shape itu. Jika tidak, kembalikan null.
    /// </summary>
    private static dynamic? GetSelectedTextShape(dynamic app)
    {
        try
        {
            if (app.Windows.Count == 0)
                return null;

            dynamic selection = app.ActiveWindow.Selection;

            if (selection.Type != MsoShapeTypeSelection)
                return null;

            if (selection.ShapeRange.Count == 0)
                return null;

            dynamic shape = selection.ShapeRange[1];

            // HasTextFrame = true berarti shape ini bisa berisi teks.
            return shape.HasTextFrame == MsoTrue ? shape : null;
        }
        catch
        {
            return null;
        }
    }


    // ============================================================
    // 4. SLIDE TUJUAN
    // ============================================================

    /// <summary>
    /// Menentukan slide tempat teks ditaruh.
    ///
    /// ATURAN (menghindari teks bertumpuk):
    ///   - Slide aktif KOSONG        -> dipakai langsung.
    ///   - Slide aktif SUDAH ADA isi -> dibuat slide kosong baru
    ///                                  tepat setelahnya.
    ///
    /// Aturan lama selalu menaruh kotak teks di posisi yang sama
    /// (30, 30), jadi Copy kedua kali menumpuk di atas yang pertama.
    /// Itulah yang membuat tulisan terlihat berantakan.
    /// </summary>
    private static dynamic PrepareTargetSlide(
        dynamic app,
        dynamic presentation)
    {
        dynamic slides = presentation.Slides;

        // -------- Cari slide yang sedang aktif --------
        dynamic? current = null;

        try
        {
            if (app.Windows.Count > 0)
                current = app.ActiveWindow.View.Slide;
        }
        catch
        {
            current = null;
        }

        // -------- Presentation belum punya slide --------
        if (current == null)
        {
            if (slides.Count == 0)
                return slides.Add(1, PpLayoutBlank);

            current = slides[1];
        }

        // -------- Slide aktif masih kosong: pakai langsung --------
        if (current.Shapes.Count == 0)
            return current;

        // -------- Slide sudah ada isi: buat slide baru --------
        int newIndex = (int)current.SlideIndex + 1;

        dynamic newSlide = slides.Add(newIndex, PpLayoutBlank);

        // Pindahkan tampilan ke slide baru agar user langsung melihatnya.
        try
        {
            app.ActiveWindow.View.GotoSlide(newIndex);
        }
        catch
        {
            // Tidak ada window aktif: abaikan.
        }

        return newSlide;
    }


    // ============================================================
    // 5. KOTAK TEKS
    // ============================================================

    /// <summary>
    /// Membuat kotak teks selebar slide (dikurangi margin).
    /// Aturan lama memakai lebar tetap 650 point sehingga teks Arab
    /// (yang rata kanan) tidak memanfaatkan lebar slide.
    /// </summary>
    private static dynamic AddFullWidthTextBox(
        dynamic presentation,
        dynamic slide)
    {
        float slideWidth =
            (float)presentation.PageSetup.SlideWidth;

        float slideHeight =
            (float)presentation.PageSetup.SlideHeight;

        // Orientation 1 = horizontal.
        dynamic textBox =
            slide.Shapes.AddTextbox(
                1,
                SlideMargin,
                SlideMargin,
                slideWidth - (SlideMargin * 2),
                slideHeight - (SlideMargin * 2));

        // Teks otomatis turun ke baris baru di tepi kotak.
        textBox.TextFrame.WordWrap = MsoTrue;

        return textBox;
    }

    /// <summary>
    /// Jika teks lebih panjang dari tinggi kotak, PowerPoint
    /// mengecilkan hurufnya otomatis (Shrink text on overflow).
    /// </summary>
    private static void EnableShrinkOnOverflow(dynamic textBox)
    {
        try
        {
            textBox.TextFrame2.AutoSize = MsoAutoSizeTextToFitShape;
        }
        catch
        {
            // Versi PowerPoint lama tidak mendukung: abaikan.
        }
    }


    // ============================================================
    // 6. ISI TEKS PER BLOK
    // ============================================================

    /// <summary>
    /// Mengisi kotak teks dengan seluruh blok pada CopyResult.
    /// Setiap blok ditambahkan sebagai paragraf baru, lalu diformat
    /// sesuai jenisnya (Arab, terjemahan, isyarat, footnote, dst.).
    /// </summary>
    private static void FillTextFrame(
        dynamic textFrame,
        CopyResult result)
    {
        // Kosongkan isi lama (penting jika kita mengisi shape terpilih).
        textFrame.TextRange.Text = string.Empty;

        bool lastWasSeparator = true;   // true: separator awal diabaikan

        foreach (CopyBlock block in result.Blocks)
        {
            // ---------------- SEPARATOR ----------------
            // Jarak antar ayat/surah berupa satu baris kosong kecil.
            // Separator beruntun digabung agar jaraknya tidak terlalu lebar.

            if (block.Type == CopyBlockType.Separator)
            {
                if (!lastWasSeparator)
                {
                    InsertBlankParagraph(textFrame);
                    lastWasSeparator = true;
                }

                continue;
            }

            if (string.IsNullOrWhiteSpace(block.Text))
                continue;

            // ---------------- BLOK BERISI TEKS ----------------

            // PowerPoint memakai "\r" sebagai pemisah paragraf.
            string text =
                block.Text
                    .Replace("\r\n", "\r")
                    .Replace("\n", "\r")
                    .TrimEnd('\r');

            // Tulisan tangan: kurung ornamen ditukar kembali      // <-- BARU
            // (khusus PowerPoint, lihat FlipBracketsForHandwritten).
            if (FlipBracketsForHandwritten &&
                block.Type == CopyBlockType.Arabic &&
                IsHandwrittenFont(block.FontFamily))
            {
                text = FlipOrnateBrackets(text);
            }

            // InsertAfter menambahkan teks di akhir dan mengembalikan
            // TextRange HANYA untuk teks yang baru dimasukkan.
            // Dengan begitu format hanya mengenai blok ini.
            dynamic range =
                textFrame.TextRange.InsertAfter(text + "\r");

            FormatBlock(range, block);

            lastWasSeparator = false;
        }

        RemoveTrailingParagraph(textFrame);
    }

    /// <summary>
    /// Menambah satu paragraf kosong berukuran kecil sebagai jarak.
    /// </summary>
    private static void InsertBlankParagraph(dynamic textFrame)
    {
        dynamic blank = textFrame.TextRange.InsertAfter("\r");

        try
        {
            blank.Font.Size = 10;
        }
        catch
        {
            // Abaikan.
        }
    }

    /// <summary>
    /// Setiap blok memasukkan "\r" di akhir, sehingga paragraf terakhir
    /// kosong. Hapus agar tidak ada baris kosong di bawah kotak teks.
    /// </summary>
    private static void RemoveTrailingParagraph(dynamic textFrame)
    {
        try
        {
            dynamic all = textFrame.TextRange;

            string text = (string)all.Text;

            if (text.EndsWith("\r"))
                all.Characters(text.Length, 1).Delete();
        }
        catch
        {
            // Tidak fatal: hanya ada satu baris kosong tambahan.
        }
    }

    /// <summary>
    /// True jika font blok adalah font tulisan tangan (Lpmq_Hafs_XX).
    /// Aturan yang sama dipakai QuranCopyFormatter.GetArabicFontFamily.
    /// </summary>
    private static bool IsHandwrittenFont(string? fontFamily)
    {
        return !string.IsNullOrWhiteSpace(fontFamily) &&
               fontFamily.StartsWith(
                   "Lpmq_Hafs_",
                   StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Menukar dua kurung ornamen:
    ///   U+FD3F ﴿  <->  U+FD3E ﴾
    /// Karakter lain tidak diubah.
    /// </summary>
    private static string FlipOrnateBrackets(string text)
    {
        char[] chars = text.ToCharArray();

        for (int i = 0; i < chars.Length; i++)
        {
            if (chars[i] == '\uFD3F')
                chars[i] = '\uFD3E';
            else if (chars[i] == '\uFD3E')
                chars[i] = '\uFD3F';
        }

        return new string(chars);
    }


    // ============================================================
    // 7. FORMAT PER JENIS BLOK
    // ============================================================

    /// <summary>
    /// Mengatur font, ukuran, arah baca, dan perataan satu blok.
    /// </summary>
    private static void FormatBlock(dynamic range, CopyBlock block)
    {
        // ---------- Ukuran ----------
        double baseSize = block.FontSize > 0 ? block.FontSize : 11;

        range.Font.Size = (float)Math.Round(baseSize * SlideFontScale);

        // ---------- Font ----------
        // Hanya diisi bila blok menentukan font (Arab dan isyarat).
        // Terjemahan memakai font bawaan template.
        if (!string.IsNullOrWhiteSpace(block.FontFamily))
            ApplyFont(range, block.FontFamily!);

        // ---------- Arah baca & perataan ----------
        switch (block.Type)
        {
            case CopyBlockType.Arabic:
                // Arab: kanan ke kiri, rata kanan.
                SetParagraph(
                    range,
                    PpDirectionRightToLeft,
                    PpAlignRight);
                break;

            case CopyBlockType.SurahInfo:
                // Referensi "(Al-Fatihah/1: 1)": di tengah.
                SetParagraph(
                    range,
                    PpDirectionLeftToRight,
                    PpAlignCenter);
                break;

            default:
                // Terjemahan, isyarat, footnote: kiri ke kanan, rata kiri.
                SetParagraph(
                    range,
                    PpDirectionLeftToRight,
                    PpAlignLeft);
                break;
        }

        // ---------- Jarak antar paragraf ----------
        try
        {
            range.ParagraphFormat.LineRuleAfter = MsoFalse; // satuan point
            range.ParagraphFormat.SpaceAfter = 6;
            range.ParagraphFormat.SpaceBefore = 0;
        }
        catch
        {
            // Abaikan.
        }
    }

    /// <summary>
    /// Menerapkan nama font ke semua jenis huruf yang dikenal PowerPoint.
    /// Ini padanan dari Font.Name + Font.NameBi pada Word.
    ///
    /// PowerPoint menyimpan nama font terpisah untuk:
    ///   - Name / NameAscii / NameOther : huruf Latin
    ///   - NameComplexScript            : huruf Arab, Ibrani, dst.
    /// Jika NameComplexScript tidak diisi, huruf Arab memakai font
    /// bawaan sehingga tulisan tangan tampil sebagai simbol aneh.
    /// </summary>
    private static void ApplyFont(dynamic range, string fontFamily)
    {
        try { range.Font.Name = fontFamily; } catch { }
        try { range.Font.NameAscii = fontFamily; } catch { }
        try { range.Font.NameOther = fontFamily; } catch { }
        try { range.Font.NameComplexScript = fontFamily; } catch { }
    }

    /// <summary>
    /// Mengatur arah baca dan perataan paragraf.
    /// Dibungkus try/catch agar satu pengaturan yang gagal
    /// tidak membatalkan seluruh proses Copy.
    /// </summary>
    private static void SetParagraph(
        dynamic range,
        int direction,
        int alignment)
    {
        try { range.ParagraphFormat.TextDirection = direction; } catch { }
        try { range.ParagraphFormat.Alignment = alignment; } catch { }
    }
}