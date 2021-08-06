using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitVenture
{
    class Endpoint
    {
        public bool enabled { get; set; }
        public string resource { get; set; }
        public IEnumerable<Dictionary<string,string>> response { get; set; }
    }
}
