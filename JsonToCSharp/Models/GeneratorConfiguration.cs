using Shared.Interfaces;
using System.Collections.Generic;

namespace JsonToCSharp.Models
{
    public class GeneratorConfiguration
    {
        public string Namespace { get; set; }

        public List<string> UsingNamespace { get; set; } = new List<string>();

        public string GeneratedPath { get; set; }

        public string LoaderClassName { get; set; }

        public ICSharpTemplateFormat CSharpTemplateFormat { get; set; }
    }
}
