using ProtocolGenerator.Models;
using System.Reflection;
using System.Text;

namespace ProtocolGenerator
{
    internal class CSharpGenerator
    {
        private readonly HandlerConfigModel _handlerConfigModel;
        private readonly ICSharpHandlerFormat _format = new DefaultCSharpHandlerFormat();
        private readonly List<ClassGenerator> _handlers = new();
        private readonly List<string> _usingNamespace;
        private readonly Dictionary<string, Type> _classTypeDatas = new();
        private readonly Dictionary<string, Type> _enumTypeDatas = new();
        public CSharpGenerator(List<string> usingNamespace,
            ICSharpHandlerFormat csharpHandlerFormat,
            HandlerConfigModel handlerConfigModel)
        {
            _handlerConfigModel = handlerConfigModel;
            if (usingNamespace != null)
            {
                _usingNamespace = usingNamespace;
            }
            else
            {
                _usingNamespace = new List<string>();
            }

            if(string.IsNullOrEmpty(csharpHandlerFormat.DeserializePacketFormat) == false)
            {
                _format.DeserializePacketFormat = csharpHandlerFormat.DeserializePacketFormat;
            }
            if(string.IsNullOrEmpty(csharpHandlerFormat.HandleClassFormat) == false)
            {
                _format.HandleClassFormat = csharpHandlerFormat.HandleClassFormat;
            }
            if (string.IsNullOrEmpty(csharpHandlerFormat.DeserializeBodyFunctionFormat) == false)
            {
                _format.DeserializeBodyFunctionFormat = csharpHandlerFormat.DeserializeBodyFunctionFormat;
            }
            if (string.IsNullOrEmpty(csharpHandlerFormat.RegistCabllackFormat) == false)
            {
                _format.RegistCabllackFormat = csharpHandlerFormat.RegistCabllackFormat;
            }
        }

        public void Load()
        {
            var assembly = Assembly.LoadFrom(Path.Combine(_handlerConfigModel.ReadAssemblyPath));

            foreach(var type in assembly.GetTypes())
            {
                if (type.IsClass && type.IsPublic)
                {
                    _classTypeDatas.Add(type.Name, type);
                }
                else if (type.IsEnum)
                {
                    _enumTypeDatas.Add(type.Name, type);
                }
            }

            foreach(var kv in _enumTypeDatas)
            {
                if(kv.Key.Equals(_handlerConfigModel.ProtocolName))
                {
                    Dictionary<string, string> protocolNames = new Dictionary<string, string>();
                    foreach(string item in Enum.GetNames(kv.Value))
                    {
                        if(item.ToLower().Equals("max"))
                        {
                            continue;
                        }

                        if(_classTypeDatas.TryGetValue(item, out Type protocolType) == false)
                        {
                            throw new Exception($"{_handlerConfigModel.ProtocolName} not found protocol class model class - {item}");
                        }
                        protocolNames.Add(item, protocolType.FullName);
                    }

                    ClassGenerator classGenerator = new(_format,
                        _usingNamespace,
                        _handlerConfigModel.Namespace,
                        _handlerConfigModel.ProtocolName,
                        _handlerConfigModel.BodyType,
                        protocolNames);

                    _handlers.Add(classGenerator);
                }
            }
        }
        public void Generate()
        {
            foreach(var item in _handlers)
            {
                var classToString = item.Build();

                for (int i = 0; i < _handlerConfigModel.GeneratedPaths.Count; ++i)
                {
                    ExportGeneratedFile(_handlerConfigModel.GeneratedPaths[i], item.ClassName, classToString);
                }
            }
            
        }
        private static void ExportGeneratedFile(string generatedPath, string generatedName, string content)
        {
              var fullPath = Path.Combine(generatedPath, $"{generatedName}.cs");

            ExportFile(fullPath, content);
        }
        private static void ExportFile(string fullPath, string content)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"{fullPath} 파일 생성");
            File.WriteAllText(fullPath, content, Encoding.UTF8);
        }
    }
}
