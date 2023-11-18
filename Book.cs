using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfOCR
{
    public class Book
    {
        public int Number { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public int Year { get; set; }

        public override string ToString()
        {
            return $"{Number}, {Title}, {Author}, {Publisher}, {Year}";
        }
    }
}