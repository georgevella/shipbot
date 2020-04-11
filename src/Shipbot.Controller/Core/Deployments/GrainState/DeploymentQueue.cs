using System.Collections.Generic;
using System.Collections.Immutable;
using Shipbot.Controller.Core.Deployments.GrainKeys;
using Shipbot.Controller.Core.Deployments.Models;

namespace Shipbot.Controller.Core.Deployments.GrainState
{
    public class DeploymentQueue
    {
        public SortedList<DeploymentActionKey, InternalDeploymentQueueItem> PendingDeploymentActions { get; } 
            = new SortedList<DeploymentActionKey, InternalDeploymentQueueItem>();   
    }
}