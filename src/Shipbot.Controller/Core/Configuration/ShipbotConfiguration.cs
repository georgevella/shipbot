using System.Collections.Generic;
using Shipbot.Controller.Core.Configuration.Apps;
using Shipbot.Controller.Core.Configuration.Git;
using Shipbot.Controller.Core.Configuration.Registry;
//using ArgoAutoDeploy.Core.Configuration.K8s;

namespace Shipbot.Controller.Core.Configuration
{
    public class ShipbotConfiguration
    {
        //public List<KubernetesConnectionDetails> Kubernetes { get; set; }
        
        public List<ContainerRegistrySettings> Registries { get; set; } = new List<ContainerRegistrySettings>();
        
        public Dictionary<string, ApplicationDefinition> Applications { get; } = new Dictionary<string, ApplicationDefinition>();
        
        public List<GitCredentialSettings> GitCredentials { get; set; } = new List<GitCredentialSettings>();
    }
}