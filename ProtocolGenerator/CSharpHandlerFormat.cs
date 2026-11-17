namespace ProtocolGenerator
{
    public interface ICSharpHandlerFormat
    {
        string HandleClassFormat { get; internal set; }
        string RegistCabllackFormat { get; internal set; }
        string DeserializePacketFormat { get; internal set; }

        string DeserializeBodyFunctionFormat { get; internal set; }
    }
    public class DefaultCSharpHandlerFormat : ICSharpHandlerFormat
    {
        public string HandleClassFormat { get; set; }

        public string RegistCabllackFormat { get; set; }

        public string DeserializePacketFormat { get; set; }

        public string DeserializeBodyFunctionFormat { get; set; }
        public DefaultCSharpHandlerFormat()
        {
            HandleClassFormat =
            "    public partial class {0}\r\n" +
            "    {{\r\n" +
            "        private static Action<{0}, {3}>[] _handlers;\r\n" +
            "        public static void Init()\r\n" +
            "        {{\r\n" +
            "            _handlers = new Action<{0}, {3}>[{1}];\r\n" +
            "{2}" +
            "        }}\r\n" +
            "        public static bool CheckProtocol(int protocol)\r\n" +
            "        {{\r\n" +
            "            if(protocol < 0 && protocol >= _handlers.Length)\r\n" +
            "            {{\r\n" +
            "                return false;\r\n" +
            "            }}\r\n" +
            "            return true;\r\n" +
            "        }}\r\n" +
            "        public void Process(int protocol, {3} body)\r\n" +
            "        {{\r\n" +
            "            _handlers[protocol](this, body);\r\n" +
            "        }}\r\n" +
            "{4}" +
            "{5}" + 
            "    }}";

            RegistCabllackFormat =
            "            _handlers[{0}] = (t, body) => t.Process{1}(body);\r\n";

            DeserializePacketFormat =
            "        protected void Process{0}({1} body)\r\n" +
            "        {{\r\n" +
            "            if(body == null)\r\n" +
            "            {{\r\n" +
            "                LogHelper.Error(\"body is null\");\r\n" +
            "                return;\r\n" +
            "            }}\r\n" +
            "            var packet = DeserializeBody<{2}>(body);\r\n" +
            "            Process(packet);\r\n" +
            "        }}\r\n";

            DeserializeBodyFunctionFormat =
            "        protected T DeserializeBody<T>({0} body)\r\n" +
            "        {{\r\n" +
            "            return System.Text.Json.JsonSerializer.Deserialize<T>(body);\r\n" +
            "        }}\r\n";

        }
    }
}
