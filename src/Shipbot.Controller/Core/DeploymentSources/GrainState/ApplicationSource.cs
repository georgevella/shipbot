using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.DeploymentSources.Models;

namespace Shipbot.Controller.Core.DeploymentSources.GrainState
{
    public abstract class ApplicationSource
    {
        public bool IsEnabled { get; set; }
        
        public bool IsActive { get; set; }
        
        public ApplicationSourceRepository Repository { get; set; }
        
        public string Path { get; set; }
        
        public DeploymentSourceMetadata Metadata { get; set; } = new DeploymentSourceMetadata();
        
        public ApplicationEnvironmentKey ApplicationEnvironment { get; set; }
        
        
    }
}