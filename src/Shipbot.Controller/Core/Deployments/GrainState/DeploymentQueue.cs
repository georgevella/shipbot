using System.Collections.Generic;
using Shipbot.Controller.Core.Deployments.GrainKeys;

namespace Shipbot.Controller.Core.Deployments.GrainState
{
    public class DeploymentQueue
    {
        public List<DeploymentActionKey> PendingDeploymentActions { get; } = new List<DeploymentActionKey>();   
    }
}