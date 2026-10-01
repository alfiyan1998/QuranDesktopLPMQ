namespace QuranKemenag.Services;

/// <summary>
/// Represents the supported database files used by the application.
/// </summary>
public enum DatabaseType
{
    /// <summary>
    /// Primary Quran database (quran.kemenag.db).
    /// </summary>
    QuranKemenag,

    /// <summary>
    /// Handwritten dataset database (quran_handwritten.db).
    /// </summary>
    Handwritten,

    /// <summary>
    /// Application data database (app.db).
    /// </summary>
    Application,

    /// <summary>
    /// Secondary database that contains symbol metadata used by the application (msi_simbol.db).
    /// This is a read-only data source at this stage and will be consumed by dedicated
    /// repository/service in a later implementation step.
    /// </summary>
    QuranSymbol,


    /// <summary>
    /// Indonesian translation database.
    /// </summary>
    Translation,

        /// <summary>
        /// Database layout Mushaf per halaman (mushaf_layout.db).
        /// Berisi posisi baris dan kata untuk setiap halaman Mushaf.
        /// </summary>
    MushafLayout,

        /// <summary>
        /// Database untuk menyimpan data Bookmark pengguna.
        /// </summary>
    Bookmark
}
