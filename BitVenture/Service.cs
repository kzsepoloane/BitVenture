using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitVenture
{
    class Service
    {
        public string baseUrl { get; set; }
        public bool enabled { get; set; }
        public string datatype { get; set; }
        public IEnumerable<Endpoint> endpoints { get; set; }
        public IEnumerable<KeyValuePair<string, string>> identifiers { get; set; }
    }
}
