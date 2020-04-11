using System.Collections.Generic;

namespace Shipbot.Controller.Core.DeploymentSources.GrainState
{
    public class HelmApplicationSource : ApplicationSource
    {
        public List<string> ValuesFiles { get; set; }
        
        public List<string> SecretFiles { get; set; }
        
    }
}