using System.Collections.Generic;
using Shipbot.Controller.Core.Configuration.Apps;
using Shipbot.Controller.Core.Configuration.Git;
using Shipbot.Controller.Core.Configuration.Registry;
//using ArgoAutoDeploy.Core.Configuration.K8s;
// ReSharper disable CollectionNeverUpdated.Global

namespace Shipbot.Controller.Core.Configuration
{
    public class ShipbotConfiguration
    {
        public bool Dryrun { get; set; }
        //public List<KubernetesConnectionDetails> Kubernetes { get; set; }
        
        public List<ContainerRegistrySettings> Registries { get; set; } = new List<ContainerRegistrySettings>();
        
        public Dictionary<string, ApplicationDefinition> Applications { get; } = new Dictionary<string, ApplicationDefinition>();
        
        public List<GitCredentialSettings> GitCredentials { get; set; } = new List<GitCredentialSettings>();

        public List<GitRepositorySettings> GitRepositories { get; set; } =
            new List<GitRepositorySettings>();

        public GithubClientSettings Github { get; set; } = new GithubClientSettings();

        public NotificationTemplates NotificationTemplates { get; set; } = new NotificationTemplates();
    }

    public class GithubClientSettings
    {
        public string Token { get; set; }
    }

    public class NotificationTemplates
    {
        public DeploymentNotifications Deployment { get; } = new DeploymentNotifications();
        public class DeploymentNotifications
        {
            public string PullRequestNotification { get; set; }
        }
    }
}