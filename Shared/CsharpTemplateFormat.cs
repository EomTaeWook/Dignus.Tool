using Shared.Interfaces;

namespace Shared
{
    public class DefaultCSharpTemplateFormat : ICSharpTemplateFormat
    {
        public string TemplateFormat { get; private set; }

        public string InnerClassFormat { get; private set; }

        public string VariableFormat { get; private set; }

        public string VariablesListFormat { get; private set; }

        public string OverrideMakeRefTemplateFormat { get; private set; }

        public string FindRefFormat { get; private set; }

        public string FindRefListFormat { get; private set; }

        public string InvalidErrorFormat { get; private set; }

        public string TemplateBaseFormat { get; private set; }

        public string TemplateContainerFormat { get; private set; }

        public string FindRefInChildForamt { get; set; }

        public DefaultCSharpTemplateFormat()
        {
            TemplateFormat = $@"    public partial class {{0}} : TemplateBase
    {{
{{1}}{{2}}{{3}}
    }}";
            InnerClassFormat = $@"        public sealed partial class Inner{{0}}
        {{
{{1}}
        }}";

            VariableFormat = $@"        public {{0}}{{1}} {{2}} {{ get; set; }}";

            VariablesListFormat = $@"        public List<{{0}}{{1}}> {{2}} {{ get; set; }} = new List<{{0}}{{1}}>();";

            OverrideMakeRefTemplateFormat = $@"
        public override void MakeRefTemplate()
        {{
{{0}}
        }}";

            FindRefListFormat = $@"         foreach(var {{1}} in {{0}})
            {{
                var template = TemplateContainer<{{3}}Template>.Find({{2}});
                if(template.Invalid() == true)
                {{
                    {{5}}
                }}
                {{4}}.Add(template);
            }}";

            FindRefFormat = $@"            {{2}} = TemplateContainer<{{0}}Template>.Find({{1}});
            if({{2}}.Invalid() == true)
            {{
                {{3}}
            }}";

            FindRefInChildForamt = $@"            foreach(var item in {{0}})
            {{
                item.{{2}} = TemplateContainer<{{3}}Template>.Find(item.{{1}});
                if (item.{{2}}.Invalid() == true)
                {{
                    {{4}}
                }}
            }}";
            InvalidErrorFormat = $@"Debug.Assert(false, $""{{0}} Ref Data not found! Ref Field : {{{{1}}}}"");";

            TemplateBaseFormat =
$@"namespace {{0}}
{{
    public class TemplateBase
    {{
        public const int InvidateTemplateId = -1;
        public int Id {{ get; set; }} = InvidateTemplateId;
        public string Name {{ get; set; }}
        public bool Invalid()
        {{
            return Id == InvidateTemplateId;
        }}
        public virtual void Combine()
        {{
        }}
        public virtual void MakeRefTemplate()
        {{
        }}
    }}
}}";
            TemplateContainerFormat = $@"
using System;
using System.Collections.Generic;
using System.IO;
namespace {{0}}
{{
    public partial class TemplateContainer<T> where T : TemplateBase, new ()
    {{
        public static IEnumerable<T> Values => _dataToMap.Values;
        private static readonly Dictionary<int, T> _dataToMap = new Dictionary<int, T>();
        private static readonly Dictionary<string, T> _nameKeyToMap = new Dictionary<string, T>();

        public static T Find(int id)
        {{
            if (_dataToMap.ContainsKey(id) == false)
            {{
                return new T();
            }}
            return _dataToMap[id];
        }}
        public static T Find(string name)
        {{
            if (_nameKeyToMap.ContainsKey(name) == false)
            {{
                return new T();
            }}
            return _nameKeyToMap[name];
        }}
        public static void Load(string path, string fileName, ITemplateDeserializer deserializer)
        {{
            string fullPath = Path.Combine(path, fileName);
            using (var stream = new StreamReader(File.OpenRead(fullPath)))
            {{
                var content = stream.ReadToEnd();
                var templateDatas = deserializer.Deserialize<T>(content);

                foreach (var template in templateDatas)
                {{
                    _dataToMap.Add(template.Id, template);
                    _nameKeyToMap.Add(template.Name, template);
                }}
            }}
        }}
        public static void Load(string fileName, Func<string, string> funcLoadJson, ITemplateDeserializer deserializer)
        {{
            var json = funcLoadJson(fileName);
            var templateDatas = deserializer.Deserialize<T>(json);

            foreach (var template in templateDatas)
            {{
                _dataToMap.Add(template.Id, template);
                _nameKeyToMap.Add(template.Name, template);
            }}
        }}
        public static void MakeRefTemplate()
        {{
            foreach (var template in _dataToMap.Values)
            {{
                template.MakeRefTemplate();
            }}
        }}
        public static void Combine()
        {{
            foreach (var template in _dataToMap.Values)
            {{
                template.Combine();
            }}
        }}
    }}
}}";
        }

    }
}
