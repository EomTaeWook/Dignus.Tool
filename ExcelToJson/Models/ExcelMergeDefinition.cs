using System.Collections.Generic;

namespace ExcelToJson.Models
{
    public class ExcelMergeDefinition
    {
        public string Name { get; set; }

        public string BaseDefine { get; set; }

        public List<string> Datas { get; set; } = new List<string>();
    }
}
