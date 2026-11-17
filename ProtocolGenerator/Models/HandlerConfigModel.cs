namespace ProtocolGenerator.Models
{
    public class HandlerConfigModel
    {
        public List<string> UsingNamespace { get; set; }
        public string Namespace { get; set; }
        public string BodyType { get; set; }
        public string ProtocolName { get; set; }
        public string ReadAssemblyPath { get; set; }
        public List<string> GeneratedPaths { get; set; }
    }
}
