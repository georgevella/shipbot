using System.Collections.Generic;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Deployments.GrainKeys;
using Shipbot.Controller.Core.Deployments.Models;

namespace Shipbot.Controller.Core.Deployments.GrainState
{
    /// <summary>
    ///     Describes a deployment of a container image to an application and one or more of it's environments.
    /// </summary>
    public class DeploymentState
    {
        public List<DeploymentActionKey> DeploymentActions { get; } 
            = new List<DeploymentActionKey>();

        public int NextDeploymentActionIndex = 0;

        public string? Application { get; set; }
        public string? ImageRepository { get; set; }
        public string? TargetTag { get; set; }
    }
}