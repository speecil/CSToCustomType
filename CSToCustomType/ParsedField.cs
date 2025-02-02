using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSToCustomType
{
    internal class ParsedField
    {
        public string Visibility { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public List<string> Attributes { get; set; } = new List<string>();
    }
}
