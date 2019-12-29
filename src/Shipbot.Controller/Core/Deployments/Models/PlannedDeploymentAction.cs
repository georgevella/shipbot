using Shipbot.Controller.Core.Apps.Models;

namespace Shipbot.Controller.Core.Deployments.Models
{
    public class PlannedDeploymentAction
    {
        public Image Image { get; set; }
        
        public string CurrentTag { get; set; }
        
        public string TargetTag { get; set; }
    }
}