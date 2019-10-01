using System.Collections.Generic;
using System.ComponentModel;
using Shipbot.Controller.Core.Configuration.ApplicationSources;

namespace Shipbot.Controller.Core.Configuration.Apps
{
    public class ApplicationDefinition
    {
        public string Name { get; set; }

        public Dictionary<string, ApplicationEnvironmentSettings> Environments { get; } =
            new Dictionary<string, ApplicationEnvironmentSettings>();

        public string SlackChannel { get; set; }
    }

    public class ApplicationEnvironmentSettings
    {
        public ApplicationSourceSettings Source { get; set; }

        public List<ImageSettings> Images { get; set; }
        
        /// <summary>
        ///     Enable to monitor the repositories tracked by this environment and have them automatically updated.
        /// </summary>
        [DefaultValue(true)] 
        public bool AutoDeploy { get; set; } = true;
        
        /// <summary>
        ///     List of environments that we can promote to from this environment
        /// </summary>
        public List<string> PromoteTargets { get; } = new List<string>();
    }
}