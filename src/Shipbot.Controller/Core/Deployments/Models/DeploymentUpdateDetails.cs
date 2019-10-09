using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Deployments
{
    public class DeploymentUpdateDetails
    {
        public DeploymentUpdate DeploymentUpdate { get; }
        public DeploymentUpdateStatus DeploymentUpdateStatus { get; set; }

        public DeploymentUpdateDetails(DeploymentUpdate deploymentUpdate, DeploymentUpdateStatus deploymentUpdateStatus)
        {
            DeploymentUpdate = deploymentUpdate;
            DeploymentUpdateStatus = deploymentUpdateStatus;
        }    
    }
}