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
        public DeploymentAction Action { get; set; }

        public DeploymentKey DeploymentKey { get; set; }
    }


}