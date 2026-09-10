using System.Collections.Generic;

namespace QuranKemenag.Models.Copy;

public sealed class CopyResult
{
    public List<CopyBlock> Blocks { get; } = new();

    public bool IsEmpty => Blocks.Count == 0;
}