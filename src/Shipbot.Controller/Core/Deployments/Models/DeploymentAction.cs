using System.Collections.Generic;
using Shipbot.Controller.Core.Apps.GrainState;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Deployments.GrainState;

namespace Shipbot.Controller.Core.Deployments.Models
{
    public class DeploymentAction
    {
        private sealed class PlannedDeploymentActionEqualityComparer : IEqualityComparer<DeploymentAction>
        {
            public bool Equals(DeploymentAction x, DeploymentAction y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.ApplicationEnvironmentKey.Application.Equals(y.ApplicationEnvironmentKey.Application) && 
                       x.ApplicationEnvironmentKey.Environment.Equals(y.ApplicationEnvironmentKey.Environment) &&
                       ApplicationEnvironmentImageKey.EqualityComparer.Equals(x.Image, y.Image) 
                       && x.CurrentTag == y.CurrentTag 
                       && x.TargetTag == y.TargetTag;
            }

            public int GetHashCode(DeploymentAction obj)
            {
                unchecked
                {
                    var hashCode = obj.ApplicationEnvironmentKey.Application.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.ApplicationEnvironmentKey.Environment.GetHashCode();
                    hashCode = (hashCode * 397) ^ ApplicationEnvironmentImageKey.EqualityComparer.GetHashCode(obj.Image);
                    hashCode = (hashCode * 397) ^ obj.CurrentTag.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.TargetTag.GetHashCode();
                    return hashCode;
                }
            }
        }
        
        public static IEqualityComparer<DeploymentAction> EqualityComparer { get; } 
            = new PlannedDeploymentActionEqualityComparer();

        public ApplicationEnvironmentKey ApplicationEnvironmentKey { get; set; }
        
        public DeploymentActionStatus Status { get; set; }
        
        public ApplicationEnvironmentImageKey Image { get; set; }
        
        public string CurrentTag { get; set; }
        
        public string TargetTag { get; set; }
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