using System.Collections.Generic;

namespace ExcelToJson.Models
{
    public class GeneratorConfiguration
    {
        public List<string> GeneratedPath { get; set; }

        public string RefPath { get; set; }

        public List<ExcelMergeDefinition> ExcelMergeDefinitions { get; set; }
    }
}
