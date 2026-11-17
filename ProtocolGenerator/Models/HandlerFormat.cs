namespace ProtocolGenerator.Models
{
    public class HandlerFormat : ICSharpHandlerFormat
    {
        public string HandleClassFormat { get; set; }

        public string RegistCabllackFormat { get; set; }

        public string DeserializePacketFormat { get; set; }

        public string DeserializeBodyFunctionFormat { get; set; }
    }
}
