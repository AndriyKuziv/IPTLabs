using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTLab2.Data
{
    public class RCFiveParams
    {
        public int wordSize { get; set; }
        public int rounds { get; set; }
        public int keySize { get; set; }

        public long MaxFileSize { get; set; }
    }
}
