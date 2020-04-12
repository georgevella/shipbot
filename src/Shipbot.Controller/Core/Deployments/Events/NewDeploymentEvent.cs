using Newtonsoft.Json;
using Shipbot.Controller.Core.Apps.GrainState;
using Shipbot.Controller.Core.Apps.Models;

namespace Shipbot.Controller.Core.Deployments.Events
{
    public class NewDeploymentEvent
    {
        [JsonConstructor]
        public NewDeploymentEvent(ApplicationEnvironmentKey applicationEnvironment, ApplicationEnvironmentImageMetadata image, string targetTag)
        {
            ApplicationEnvironment = applicationEnvironment;
            Image = image;
            TargetTag = targetTag;
        }

        public ApplicationEnvironmentKey ApplicationEnvironment { get; }
        public ApplicationEnvironmentImageMetadata Image { get; }
        public string TargetTag { get; }
    }
}