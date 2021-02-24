using System;
using System.Collections.Generic;
using System.ComponentModel;
using Shipbot.Controller.Core.Configuration.DeploymentManifests;

namespace Shipbot.Controller.Core.Configuration.Apps
{
    public class ApplicationDefinition
    {
        [Obsolete("This should not be used anymore since the configuration schema has changed to use a map instead of list")]
        public string Name { get; set; }
        
        public DeploymentManifestSourceSettings Source { get; set; }

        public List<ImageSettings> Images { get; set; }

        [DefaultValue(true)] 
        public bool AutoDeploy { get; set; } = true;

        public string SlackChannel { get; set; }
    }
}