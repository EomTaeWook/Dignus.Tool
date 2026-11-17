using System.Collections.Generic;

namespace Shared
{
    public class AssemblyGenerate
    {
        public Dictionary<string, string> EnumNamespace { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, List<string>> EnumValueDatas { get; set; } = new Dictionary<string, List<string>>();
    }
}
