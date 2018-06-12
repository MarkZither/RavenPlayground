using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RavenPlayground.Lib.Models
{

    public class GutBook
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Language { get; set; }
        public string Text { get; set; }
        public double Version { get; set; }
        public string Query { get; set; }
    }
}
