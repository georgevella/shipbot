using System;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Deployments.Models;

namespace Shipbot.Controller.Core.Deployments.GrainState
{
    /// <summary>
    ///     Describes a deployment change to execute by one of the deployment source updaters.
    /// </summary>
    public class DeploymentActionState
    {
        public Image Image { get; set; }
        
        public string CurrentTag { get; set; }
        
        public string TargetTag { get; set; }
        
        public ApplicationEnvironmentKey ApplicationEnvironmentKey { get; set; }
        
        public DeploymentActionStatus DeploymentActionStatus { get; set; }
        
        public DeploymentKey DeploymentKey { get; set; }
    }

    public enum DeploymentActionStatus
    {
        Created        = 0,
        Pending,
        Starting,
        UpdatingManifests,
        Synchronizing,
        Synchronized,
        Complete,
        Promoting,
        Promoted,
        Failed
    }
}