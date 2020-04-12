using Shipbot.Controller.Core.Deployments.GrainKeys;
using Shipbot.Controller.Core.Deployments.GrainState;
using Shipbot.Controller.Core.Deployments.Models;

namespace Shipbot.Controller.Core.Deployments.Events
{
    public class DeploymentStatusChangeEvent
    {
        public DeploymentStatusChangeEvent(DeploymentState fromStatus, DeploymentState status, DeploymentKey deploymentKey)
        {
            FromStatus = fromStatus;
            ToStatus = status;
            DeploymentKey = deploymentKey;
        }
        
        public DeploymentKey DeploymentKey { get; }

        public DeploymentState FromStatus { get; }
        
        public DeploymentState ToStatus { get; }
    }

    public class DeploymentActionStatusChangeEvent
    {
        public DeploymentActionStatusChangeEvent(DeploymentActionKey actionKey, DeploymentActionStatus fromStatus, DeploymentActionStatus toStatus)
        {
            ActionKey = actionKey;
            FromStatus = fromStatus;
            ToStatus = toStatus;
        }
        
        public DeploymentActionKey ActionKey { get; }
        
        public DeploymentActionStatus FromStatus { get; }
        
        public DeploymentActionStatus ToStatus { get; }
    }

    public class DeploymentQueueChange
    {
        
    }
}