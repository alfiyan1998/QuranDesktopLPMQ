using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuranKemenag.Models;
using QuranKemenag.Repositories;

namespace QuranKemenag.Services;

public sealed class BookmarkService
{
    private readonly BookmarkRepository _repository;

    public BookmarkService(BookmarkRepository repository)
    {
        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Menambahkan bookmark untuk sebuah ayat.
    /// </summary>
    public async Task<long> AddAsync(
        int surah,
        int ayat,
        string? note = null)
    {
        return await _repository
            .AddAsync(surah, ayat, note)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Mengambil bookmark untuk ayat tertentu.
    /// </summary>
    public async Task<Bookmark?> GetAsync(
        int surah,
        int ayat)
    {
        return await _repository
            .GetAsync(surah, ayat)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Mengambil seluruh bookmark.
    /// </summary>
    public async Task<IReadOnlyList<Bookmark>> GetAllAsync()
    {
        return await _repository
            .GetAllAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Menghapus bookmark berdasarkan ID.
    /// </summary>
    public async Task DeleteAsync(long id)
    {
        await _repository
            .DeleteAsync(id)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Memperbarui catatan untuk bookmark berdasarkan ID.
    /// </summary>
    public async Task UpdateNoteAsync(long id, string? note)
    {
        await _repository
            .UpdateNoteAsync(id, note)
            .ConfigureAwait(false);
    }
}