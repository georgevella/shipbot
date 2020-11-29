using System.Collections.Concurrent;
using System.IO;
using Shipbot.Controller.Core.ApplicationSources.Models;
using Shipbot.Controller.Core.Configuration.ApplicationSources;
using Shipbot.Models;
using ApplicationSourceRepository = Shipbot.Controller.Core.ApplicationSources.Models.ApplicationSourceRepository;

namespace Shipbot.Controller.Core.ApplicationSources
{
    public class ApplicationSourceTrackingContext
    {
        public string ApplicationName { get; }
        
        public ApplicationSource ApplicationSource { get; set; }

        public bool AutoDeploy { get; set; } = true;

        public string GitRepositoryPath { get; }

        public string GitSyncJobId { get; } 

        public ApplicationSourceTrackingContext(string applicationName, ApplicationSource applicationSource, string checkoutPath)
        {
            GitRepositoryPath = checkoutPath;
            ApplicationName = applicationName;
            ApplicationSource = applicationSource;
            GitSyncJobId = $"{ApplicationName}-gitsync-job";
        }
    }
}