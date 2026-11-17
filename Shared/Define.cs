using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Shared
{
    public class Define
    {
        public string Name { get; set; }

        public int Count { get; set; }

        public bool Required { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DataType DataType { get; set; }

        public List<string> Members { get; set; } = new List<string>();

        public string RefTemplate { get; set; }

        public string RefTemplateField { get; set; }

        public string ConditionField { get; set; }

        public List<Condition> Conditions { get; set; } = new List<Condition>();
    }
}
