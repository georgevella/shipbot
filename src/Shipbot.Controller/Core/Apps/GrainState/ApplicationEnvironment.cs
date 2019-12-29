using System.Collections.Generic;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Configuration.ApplicationSources;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Apps.GrainState
{
    public class ApplicationEnvironment
    {
        public List<Image> Images { get; set; }
        
        public HashSet<ImageUpdateSettings> ImageUpdateSettings { get; set; } = new HashSet<ImageUpdateSettings>(Shipbot.Controller.Core.Apps.Models.ImageUpdateSettings.EqualityComparer);
        
        public HashSet<DeployedImageTag> CurrentImageTags { get; set; }
        
        public bool AutoDeploy { get; set; }
        public List<string> PromotionEnvironments { get; set; }
        public ApplicationSourceSettings ApplicationSourceSettings { get; set; }

        public ApplicationEnvironment()
        {
            CurrentImageTags = new HashSet<DeployedImageTag>(DeployedImageTag.EqualityComparer);
        }
    }
}