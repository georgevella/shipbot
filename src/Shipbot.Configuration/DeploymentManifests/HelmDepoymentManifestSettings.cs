using System.Collections.Generic;

namespace Shipbot.Controller.Core.Configuration.DeploymentManifests
{
    public class HelmDepoymentManifestSettings
    {
        public List<string> ValueFiles { get; set; }
        
        public List<string> Secrets { get; set; }
    }
}