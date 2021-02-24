using System.Collections.Concurrent;
using System.IO;
using Shipbot.Controller.Core.ApplicationSources.Models;
using Shipbot.Models;
using ApplicationSourceRepository = Shipbot.Controller.Core.ApplicationSources.Models.ApplicationSourceRepository;

namespace Shipbot.Controller.Core.ApplicationSources
{
    public class DeploymentManifestSourceTrackingContext
    {
        public string ApplicationName { get; }
        
        public ApplicationSource ApplicationSource { get; set; }

        public bool AutoDeploy { get; set; } = true;

        public string GitRepositoryPath { get; }

        public string GitSyncJobId { get; } 

        public DeploymentManifestSourceTrackingContext(string applicationName, ApplicationSource applicationSource, string checkoutPath)
        {
            GitRepositoryPath = checkoutPath;
            ApplicationName = applicationName;
            ApplicationSource = applicationSource;
            GitSyncJobId = $"{ApplicationName}-gitsync-job";
        }
    }
}