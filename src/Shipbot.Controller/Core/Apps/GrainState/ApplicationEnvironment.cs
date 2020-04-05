using System.Collections.Generic;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Configuration.ApplicationSources;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Apps.GrainState
{
    public class ApplicationEnvironment
    {
        public HashSet<Image> Images { get; set; } 
            = new HashSet<Image>(Image.EqualityComparer);
        
        public HashSet<DeployedImageTag> CurrentImageTags { get; set; }
            = new HashSet<DeployedImageTag>(DeployedImageTag.EqualityComparer);
        
        public bool AutoDeploy { get; set; }
        public List<string> PromotionEnvironments { get; set; }
        public ApplicationSourceSettings ApplicationSourceSettings { get; set; }

    }
}