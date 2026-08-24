using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuranKemenag.Services.Page
{
    public sealed class MushafLineLayout
    {
        public int LineNumber { get; set; }

        public List<MushafLineItem> Items { get; } = new();

        public double Width { get; set; }

        public double Height { get; set; }

        public double Top { get; set; }
    }

    public sealed class MushafLineItem
    {
        public string Text { get; set; } = string.Empty;

        public int AyatNumber { get; set; }

        public bool IsAyatEnd { get; set; }

        public double Width { get; set; }

        public double X { get; set; }
    }
}
