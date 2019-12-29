using System.Collections.Generic;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Deployments.Models;

namespace Shipbot.Controller.Core.Deployments.GrainState
{
    /// <summary>
    ///     Describes a deployment of a container image to an application and one or more of it's environments.
    /// </summary>
    public class DeploymentState
    {
        
        public HashSet<DeploymentActionKey> DeploymentActions { get; } = new HashSet<DeploymentActionKey>();
        
        public List<PlannedDeploymentAction> DeploymentPlan { get; } = new List<PlannedDeploymentAction>();

        public string Application { get; set; }
        public string ImageRepository { get; set; }
        public string TargetTag { get; set; }
    }
}