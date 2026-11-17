using System.Collections.Generic;

namespace JsonToCSharp.Models
{
    internal class Variables
    {
        public bool IsList { get; set; }
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string DataTypeString { get; set; }

        public List<Variables> Members = new List<Variables>();
        
        public RefVariable RefVariable { get; set; }
    }
    
}
