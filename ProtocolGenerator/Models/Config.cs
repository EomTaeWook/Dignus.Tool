namespace ProtocolGenerator.Models
{
    public class Config
    {
        public List<HandlerConfigModel> Protocols { get; set; }

        public HandlerFormat HandlerFormat { get; set; } = new HandlerFormat();
        public List<string> UsingNamespace { get; set; } = new List<string>();
    }
}
