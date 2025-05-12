using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecieptJsonParser
{
    public class Constants
    {
        // kMaxTableCellXheight represents the average maximum height of a table cell in the OCR result, used as a multiplier.
        public const double kMaxTableCellXheight = 2.0;
        // tesseract is noise value 0.3 but we need to increase it to 0.5
        // Becauce we need line value
        public  const double kAllowBlobHeight = 0.5;

    }
}