using System;

namespace QuranKemenag.Models
{
    public enum SurahSectionLayoutType
    {
        AlFatiha,
        AlifLamMim,
        StandardStart,
        MiddleSwitch,
        Standard
    }

    public sealed class SurahSectionLayout
    {
        public int SurahNumber { get; init; }

        public int StartAyatNumber { get; init; }

        public SurahStartPosition Position { get; init; }

        public bool ShowBismillah { get; init; }

        public bool IsAlFatiha { get; init; }

        public bool IsAlifLamMim { get; init; }

        public SurahSectionLayoutType LayoutType { get; init; }
    }
}
