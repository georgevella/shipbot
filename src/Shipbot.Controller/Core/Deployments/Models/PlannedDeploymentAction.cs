using System.Collections.Generic;
using Shipbot.Controller.Core.Apps.Models;

namespace Shipbot.Controller.Core.Deployments.Models
{
    public class PlannedDeploymentAction
    {
        private sealed class PlannedDeploymentActionEqualityComparer : IEqualityComparer<PlannedDeploymentAction>
        {
            public bool Equals(PlannedDeploymentAction x, PlannedDeploymentAction y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Application.Equals(y.Application) && 
                       x.Environment.Equals(y.Environment) &&
                       Image.EqualityComparer.Equals(x.Image, y.Image) 
                       && x.CurrentTag == y.CurrentTag 
                       && x.TargetTag == y.TargetTag;
            }

            public int GetHashCode(PlannedDeploymentAction obj)
            {
                unchecked
                {
                    var hashCode = obj.Application.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.Environment.GetHashCode();
                    hashCode = (hashCode * 397) ^ Image.EqualityComparer.GetHashCode(obj.Image);
                    hashCode = (hashCode * 397) ^ obj.CurrentTag.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.TargetTag.GetHashCode();
                    return hashCode;
                }
            }
        }

        public static IEqualityComparer<PlannedDeploymentAction> EqualityComparer { get; } 
            = new PlannedDeploymentActionEqualityComparer();

        public string Application { get; set; }
        
        public string Environment { get; set; }
        
        public Image Image { get; set; }
        
        public string CurrentTag { get; set; }
        
        public string TargetTag { get; set; }
    }
}