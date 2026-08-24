using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuranKemenag.Models
{
    public class PageLine
    {
        public int LineNumber { get; set; }

        public List<Ayat> Ayats { get; set; } = new();
    }
}
