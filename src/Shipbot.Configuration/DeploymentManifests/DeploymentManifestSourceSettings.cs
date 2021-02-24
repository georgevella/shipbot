using Shipbot.Controller.Core.Configuration.Git;

namespace Shipbot.Controller.Core.Configuration.DeploymentManifests
{
    /// <summary>
    ///     Configuration structure describing a git repository containing deployment manifests.
    /// </summary>
    public class DeploymentManifestSourceSettings
    {
        /// <summary>
        ///     Type of deployment manifests present in this repository.
        /// </summary>
        public DeploymentManifestType Type { get; set; }
        
        /// <summary>
        ///     
        /// </summary>
        public GitRepositorySettings Repository { get; set; }
        
        public string Path { get; set; }
        
        public HelmDepoymentManifestSettings Helm { get; set; }
    }
}