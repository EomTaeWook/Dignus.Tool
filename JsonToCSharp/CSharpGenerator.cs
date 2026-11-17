using JsonToCSharp.Models;
using Shared;
using Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JsonToCSharp
{
    internal class CSharpGenerator
    {
        private const string Tab = "    ";
        private const string GeneratedTemplateContainerLoad = "TemplateContainer<{0}>.Load(path, \"{1}.json\", deserializer);";
        private const string GeneratedTemplateContainerLoadToFunc = "TemplateContainer<{0}>.Load(\"{1}.json\", funcLoadJson, deserializer);";
        private const string GeneratedTemplateContainerMakeRefTemplate = "TemplateContainer<{0}>.MakeRefTemplate();";
        private const string GeneratedTemplateContainerCombine = "TemplateContainer<{0}>.Combine();";
        private const string GeneratedTemplateName = "{0}Template";

        private readonly GeneratorConfiguration _generatorConfiguration;

        private readonly Dictionary<string, string> _addedTemplate = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _jsonFileNameToMap = new Dictionary<string, string>();
        private readonly EnumContainer _enumContainer;
        private readonly ICSharpTemplateFormat _csharpTemplateFormat;
        public CSharpGenerator(GeneratorConfiguration generatorConfiguration, EnumContainer enumContainer)
        {
            this._generatorConfiguration = generatorConfiguration;
            this._enumContainer = enumContainer;
            _csharpTemplateFormat = _generatorConfiguration.CSharpTemplateFormat;
        }
        public void Read(string path)
        {
            var json = File.ReadAllText(path);

            var fileName = Path.GetFileNameWithoutExtension(path);

            List<Define> defines = JsonHelper.Deserialize<List<Define>>(json);

            var templateName = string.Format(GeneratedTemplateName, fileName);

            var templateString = GetTemplateString(templateName, defines);

            _addedTemplate.Add(templateName, templateString);
            _jsonFileNameToMap.Add(templateName, fileName);
        }
        public void Generate()
        {
            var info = new DirectoryInfo(_generatorConfiguration.GeneratedPath);
            if (info.Exists == false)
            {
                info.Create();
            }
            var removeFiles = new List<FileInfo>();

            foreach (var file in info.GetFiles())
            {
                if (_addedTemplate.ContainsKey(Path.GetFileNameWithoutExtension(file.Name)) == false)
                {
                    removeFiles.Add(file);
                }
            }
            foreach (var item in removeFiles)
            {
                item.Delete();
            }

            var baseTemplate = MakeBaseTemplate();
            ExportGeneratedFile("BaseTemplate", baseTemplate);

            var templateContainer = MakeTemplateContainer();
            ExportGeneratedFile("TemplateContainer", templateContainer);

            foreach (var kv in _addedTemplate)
            {
                ExportGeneratedFile(kv.Key, kv.Value);
            }

            var loaderContent = MakeTemplateLoader();
            ExportGeneratedFile(_generatorConfiguration.LoaderClassName, loaderContent);
        }

        private string MakeBaseTemplate()
        {
            return TemplateParse.Parse(_csharpTemplateFormat.TemplateBaseFormat, _generatorConfiguration.Namespace);
        }

        private string MakeTemplateContainer()
        {
            return TemplateParse.Parse(_csharpTemplateFormat.TemplateContainerFormat, _generatorConfiguration.Namespace);
        }

        private string MakeTemplateLoader()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in _generatorConfiguration.UsingNamespace)
            {
                sb.AppendLine(item);
            }
            sb.AppendLine();
            sb.AppendLine($"namespace {_generatorConfiguration.Namespace}");
            sb.AppendLine($"{{");

            sb.AppendLine($"{Tab}public interface ITemplateDeserializer");
            sb.AppendLine($"{Tab}{{");
            sb.AppendLine($"{Tab}{Tab}IEnumerable<T> Deserialize<T>(string json) where T : TemplateBase, new();");
            sb.AppendLine($"{Tab}}}");

            sb.AppendLine($"{Tab}public partial class {_generatorConfiguration.LoaderClassName}");
            sb.AppendLine($"{Tab}{{");
            sb.AppendLine($"{Tab}{Tab}public static void Load(string path, ITemplateDeserializer deserializer)");
            sb.AppendLine($"{Tab}{Tab}{{");
            foreach (var item in _addedTemplate)
            {
                sb.Append($"{Tab}{Tab}{Tab}");
                var jsonFileName = _jsonFileNameToMap[item.Key];
                sb.AppendLine($"{string.Format(GeneratedTemplateContainerLoad, item.Key, jsonFileName)}");
            }
            sb.AppendLine($"{Tab}{Tab}}}");
            sb.AppendLine($"{Tab}{Tab}public static void Load(Func<string, string> funcLoadJson, ITemplateDeserializer deserializer)");
            sb.AppendLine($"{Tab}{Tab}{{");
            foreach (var item in _addedTemplate)
            {
                sb.Append($"{Tab}{Tab}{Tab}");
                var jsonFileName = _jsonFileNameToMap[item.Key];
                sb.AppendLine($"{string.Format(GeneratedTemplateContainerLoadToFunc, item.Key, jsonFileName)}");
            }
            sb.AppendLine($"{Tab}{Tab}}}");
            sb.AppendLine($"{Tab}{Tab}public static void MakeRefTemplate()");
            sb.AppendLine($"{Tab}{Tab}{{");
            foreach (var item in _addedTemplate)
            {
                sb.Append($"{Tab}{Tab}{Tab}");
                sb.AppendLine($"{string.Format(GeneratedTemplateContainerMakeRefTemplate, item.Key)}");
            }
            sb.AppendLine();
            foreach (var item in _addedTemplate)
            {
                sb.Append($"{Tab}{Tab}{Tab}");
                sb.AppendLine($"{string.Format(GeneratedTemplateContainerCombine, item.Key)}");
            }

            sb.AppendLine($"{Tab}{Tab}}}");
            sb.AppendLine($"{Tab}}}");
            sb.AppendLine($"}}");
            return sb.ToString();
        }

        private string GetTemplateString(string className, List<Define> defines)
        {
            VariablesContainer variablesContainer = new VariablesContainer(_enumContainer);
            variablesContainer.Build(defines);

            ClassGenerator classGenerator = new ClassGenerator(_csharpTemplateFormat,
                _generatorConfiguration.UsingNamespace,
                _generatorConfiguration.Namespace,
                className,
                variablesContainer);

            return classGenerator.Build();

        }

        private void ExportGeneratedFile(string generatedName, string content)
        {
            var fullPath = Path.Combine(_generatorConfiguration.GeneratedPath, $"{generatedName}.cs");

            ExportFile(fullPath, content);
        }
        private void ExportFile(string fullPath, string content)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"{fullPath} 파일 생성");

            File.WriteAllText(fullPath, content, Encoding.UTF8);
        }
    }
}
