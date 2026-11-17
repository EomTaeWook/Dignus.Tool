namespace DllDeploy.Models
{
    public class DeployInfoModel
    {
        public string Version { get; set; }
        public string DeployPath { get; set; }
        public List<string> IncludeDll { get; set; } = new List<string>();
        public List<string> ExternalFile { get; set; } = new List<string>();
    }
}
