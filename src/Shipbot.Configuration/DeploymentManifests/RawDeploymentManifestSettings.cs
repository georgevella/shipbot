namespace Shipbot.Controller.Core.Configuration.DeploymentManifests
{
    public class RawDeploymentManifestSettings
    {
        public string File { get; set; }
        
        public PreviewReleaseSettings PreviewRelease { get; set; }
    }
}