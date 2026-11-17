namespace JsonToCSharp.Models
{
    public class Config
    {
        public string CSharpTemplateFormatPath { get; set; }
        public string ReadRefPath { get; set; }
        public string GeneratedPath { get; set; }
        public string ProjectPath { get; set; }

        public string Namespace { get; set; }
    }
}
