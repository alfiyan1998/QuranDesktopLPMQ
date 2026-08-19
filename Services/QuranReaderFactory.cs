using System;
using Microsoft.Extensions.DependencyInjection;
using QuranKemenag.Models;
using QuranKemenag.Repositories;

namespace QuranKemenag.Services;

/// <summary>
/// Factory that creates IQuranReaderService instances based on DisplayMode.
/// Uses IServiceProvider to obtain required repository dependencies.
/// </summary>
public sealed class QuranReaderFactory : IQuranReaderFactory
{
    private readonly IServiceProvider _provider;

    public QuranReaderFactory(IServiceProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public IQuranReaderService Create(DisplayMode mode)
    {
        System.Diagnostics.Debug.WriteLine("Factory: Creating " + mode.ToString() + " ReaderService");
        return mode switch
        {
            DisplayMode.HandwrittenManuscript => (IQuranReaderService)_provider.GetRequiredService(typeof(HandwrittenManuscriptReaderService)),
            DisplayMode.ComputerFont => (IQuranReaderService)_provider.GetRequiredService(typeof(ComputerFontReaderService)),
            _ => throw new NotSupportedException($"DisplayMode {mode} not supported")
        };
    }
}
