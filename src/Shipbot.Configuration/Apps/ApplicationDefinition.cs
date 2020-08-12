using System.Collections.Generic;
using System.ComponentModel;
using Shipbot.Controller.Core.Configuration.ApplicationSources;

namespace Shipbot.Controller.Core.Configuration.Apps
{
    public class ApplicationDefinition
    {
        public string Name { get; set; }
        
        public ApplicationSourceSettings Source { get; set; }

        public List<ImageSettings> Images { get; set; }

        [DefaultValue(true)] 
        public bool AutoDeploy { get; set; } = true;

        public string SlackChannel { get; set; }
    }
}