using System.Collections.Generic;
using Shipbot.Controller.Core.Apps.Models;

namespace Shipbot.Controller.Core.DeploymentSources.Models
{
    public class HelmApplicationSource : ApplicationSource
    {
        public List<string> ValuesFiles { get; set; }
        
        public List<string> SecretFiles { get; set; }
        
    }
}