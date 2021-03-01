using System.Collections.Generic;
using System.Linq;
using Shipbot.Controller.Core.Configuration.DeploymentManifests;

namespace Shipbot.Controller.Core.ApplicationSources.Models
{
    public class HelmDeploymentManifest : DeploymentManifest
    {
        public IEnumerable<string> ValuesFiles { get; }
        
        public IEnumerable<string> Secrets { get; }

        public HelmDeploymentManifest(
            string application, 
            DeploymentManifestSource repository, 
            string path,
            IEnumerable<string> valueFiles,
            IEnumerable<string> secretFiles
            ) : base(application, repository, path)
        {
            ValuesFiles = valueFiles?.ToList() ?? Enumerable.Empty<string>();
            Secrets = secretFiles?.ToList() ?? Enumerable.Empty<string>();
        }
    }

    public class RawDeploymentManifest : DeploymentManifest
    {
        public IEnumerable<string> Files { get; }
        
        public PreviewReleaseSettings PreviewRelease { get; }
        
        public RawDeploymentManifest(
            string application, 
            DeploymentManifestSource repository,
            string path,
            IEnumerable<string> files, 
            PreviewReleaseSettings previewRelease) : base(application, repository, path)
        {
            PreviewRelease = previewRelease;
            Files = files?.ToList() ?? Enumerable.Empty<string>();
        }
    }
}