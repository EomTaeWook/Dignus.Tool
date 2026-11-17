using Shared;
using System.IO;

namespace JsonToCSharp
{
    internal class EnumRefReader
    {
        public EnumContainer Load(string path)
        {
            var fullPath = Path.Combine(path, Consts.RefName, Consts.EnumRefFileName);

            var json = File.ReadAllText(fullPath);

            AssemblyGenerate assemblyGenerate = JsonHelper.Deserialize<AssemblyGenerate>(json);

            var enumContainer = new EnumContainer();

            foreach (var kv in assemblyGenerate.EnumValueDatas)
            {
                var namespaceName = assemblyGenerate.EnumNamespace[kv.Key];

                enumContainer.AddNamespace(kv.Key, namespaceName);
                foreach (var value in kv.Value)
                {
                    enumContainer.Add(kv.Key, value);
                }
            }
            return enumContainer;
        }
    }
}
