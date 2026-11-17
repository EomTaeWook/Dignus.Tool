using System.Collections.Generic;

namespace ExcelToJson.Models
{
    public class Config
    {
        public string ReadAssemblyPath { get; set; }
        public string ReadPath { get; set; }
        public string OutputRefPath { get; set; }
        public List<string> OutputTemplatePath { get; set; } = new List<string>();

        public List<ExcelMergeDefinition> ExcelMergeDefinitions { get; set; } = new List<ExcelMergeDefinition>();
    }
}
