namespace Shipbot.Controller.Core.Configuration.ApplicationSources
{
    public class DeploymentManifestSettings
    {
        public DeploymentManifestType Type { get; set; }
        
        public DeploymentManifestRepositorySettings Repository { get; set; }
        
        public string Path { get; set; }
        
        public HelmDepoymentManifestSettings Helm { get; set; }
    }
}