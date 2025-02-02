using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSToCustomType
{
    internal class ParsedMethod
    {
        public string Visibility { get; set; }
        public string ReturnType { get; set; }
        public string Name { get; set; }
        public List<string> Parameters { get; set; } = new List<string>();
        public List<string> Attributes { get; set; } = new List<string>();
        public bool IsOverride { get; set; } = false;
    }
}
