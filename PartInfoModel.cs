using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;

namespace PartsInfo
{
    public class PartInfoModel
    {
        public string PartNumber { get; set; }
        public string ImagePath { get; set; }
        public string Drawing { get; set; }
        public string Rev { get; set; }
        public string Description { get; set; }
        public string Material { get; set; }
        public string Supplier { get; set; }
        public decimal MaterialCost { get; set; }
        public decimal PriceEach { get; set; }
        public string Source { get; set; }
        public string Comment { get; set; }

        public BitmapSource Image { get; set; }
    }
}
