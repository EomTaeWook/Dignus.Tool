namespace DllDeploy.Models
{
    public class Config
    {
        public Dictionary<string, string> VersionPath { get; set; }
        public List<DeployInfoModel> DeployInfos { get; set; }

    }
}
