using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSToCustomType
{
    internal class ParsedType
    {
        public string Name { get; set; }
        public string NamespaceName { get; set; }
        public string BaseClass { get; set; }
        public List<string> Interfaces { get; set; } = new List<string>();
        public List<string> Attributes { get; set; } = new List<string>();
        public List<ParsedField> Fields { get; set; } = new List<ParsedField>();
        public List<ParsedMethod> Methods { get; set; } = new List<ParsedMethod>();
    }
}
