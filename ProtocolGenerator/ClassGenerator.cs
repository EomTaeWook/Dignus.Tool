using ProtocolGenerator;
using System.Text;

namespace ProtocolGenerator
{
    internal class ClassGenerator
    {
        public string ClassName { get; private set; }
        private readonly string _namespaceName;
        
        private readonly ICollection<string> _usingNamespaceDatas;
        private readonly ICSharpHandlerFormat _csharpHandlerFormat;
        private readonly string _protocolBodyType;
        private readonly Dictionary<string, string> _protocolNames;
 
        public ClassGenerator(
            ICSharpHandlerFormat cSharpTemplateFormat,
            ICollection<string> usingNamespaceDatas,
            string namespaceName,
            string className,
            string protocolBodyType,
            Dictionary<string, string> protocolNames)
        {
            this._namespaceName = namespaceName;
            this.ClassName = $"{className}Handler";
            this._usingNamespaceDatas = usingNamespaceDatas;
            this._csharpHandlerFormat = cSharpTemplateFormat;
            this._protocolBodyType = protocolBodyType;

            _protocolNames = protocolNames;
        }

        public string Build()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(MakeNamespace());
            sb.AppendLine("{");
            sb.AppendLine(MakeClassString());
            sb.AppendLine("}");
            return sb.ToString();
        }
        private string MakeClassString()
        {
            var registCabllackToString = MakeRegistCabllackFormat();

            var deserializePacketToString = MakeDeserializePacketFormat();

            var deserializeBodyFunctionToSting = MakeDeserializeBodyFunctionFormat();

            var classToStrings = string.Format(_csharpHandlerFormat.HandleClassFormat,
                ClassName,
                _protocolNames.Count.ToString(),
                registCabllackToString,
                _protocolBodyType,
                deserializePacketToString,
                deserializeBodyFunctionToSting);
            return classToStrings;
        }
        public string MakeDeserializeBodyFunctionFormat()
        {
            var sb = new StringBuilder();
            sb.Append(string.Format(_csharpHandlerFormat.DeserializeBodyFunctionFormat,
                    _protocolBodyType));

            return sb.ToString();
        }
        public string MakeDeserializePacketFormat()
        {
            var sb = new StringBuilder();

            foreach(var item in _protocolNames) 
            {
                sb.Append(string.Format(_csharpHandlerFormat.DeserializePacketFormat,
                    item.Key,
                    _protocolBodyType,
                    item.Value));
            }

            return sb.ToString();
        }

        private string MakeRegistCabllackFormat()
        {
            var sb = new StringBuilder();
            var index = 0;
            foreach(var item in _protocolNames)
            {
                sb.Append(string.Format(_csharpHandlerFormat.RegistCabllackFormat,
                    index++,
                    item.Key));
            }

            return sb.ToString();
        }
        private string MakeNamespace()
        {
            var sb = new StringBuilder();
            if(_usingNamespaceDatas.Count > 0)
            {
                foreach (var item in _usingNamespaceDatas)
                {
                    sb.AppendLine(item);
                }
                sb.AppendLine();
            }
            sb.AppendLine($"namespace {_namespaceName}");

            return sb.ToString();
        }
    }
}
