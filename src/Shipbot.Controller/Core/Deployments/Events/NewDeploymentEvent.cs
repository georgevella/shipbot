using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Shipbot.Controller.Core.Apps.GrainState;
using Shipbot.Controller.Core.Apps.Models;

namespace Shipbot.Controller.Core.Deployments.Events
{
    public class NewDeploymentEvent
    {
        [JsonConstructor]
        public NewDeploymentEvent(
            ApplicationEnvironmentKey applicationEnvironment, 
            ApplicationEnvironmentImageKey image,
            string currentTag,
            string targetTag,
            bool isPromotable,
            IEnumerable<string> promoteEnvironmentSequence, 
            bool isManuallyStarted)
        {
            ApplicationEnvironment = applicationEnvironment;
            Image = image;
            TargetTag = targetTag;
            IsPromotable = isPromotable;
            IsManuallyStarted = isManuallyStarted;

            PromotableEnvironments = promoteEnvironmentSequence.ToArray();
            CurrentTags[applicationEnvironment] = currentTag;
        }
        
        public bool IsManuallyStarted { get; }
        
        public bool IsPromotable { get; }
        
        public string[] PromotableEnvironments { get; }

        public ApplicationEnvironmentKey ApplicationEnvironment { get; }
        public ApplicationEnvironmentImageKey Image { get; }
        public string TargetTag { get; }

        public Dictionary<ApplicationEnvironmentKey, string> CurrentTags { get; } =
            new Dictionary<ApplicationEnvironmentKey, string>(ApplicationEnvironmentKey.EqualityComparer);
    }
}