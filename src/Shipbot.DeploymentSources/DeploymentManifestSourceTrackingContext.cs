using System.Collections.Concurrent;
using System.IO;
using Shipbot.Controller.Core.ApplicationSources.Models;
using Shipbot.Models;

namespace Shipbot.Controller.Core.ApplicationSources
{
    public class DeploymentManifestSourceTrackingContext
    {
        public string ApplicationName { get; }
        
        public DeploymentManifest DeploymentManifest { get; set; }

        public bool AutoDeploy { get; set; } = true;

        public string GitRepositoryPath { get; }

        public string GitSyncJobId { get; } 

        public DeploymentManifestSourceTrackingContext(string applicationName, DeploymentManifest deploymentManifest, string checkoutPath)
        {
            GitRepositoryPath = checkoutPath;
            ApplicationName = applicationName;
            DeploymentManifest = deploymentManifest;
            GitSyncJobId = $"{ApplicationName}-gitsync-job";
        }
    }
}