using System.Collections.Generic;
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
        
        public HelmDeploymentManifestSettings Helm { get; set; }
        public RawDeploymentManifestSettings Raw { get; set; }
    }

    public class PreviewReleaseSettings
    {
        private bool? _basePreleaseOnFile;
        public string TemplateFile { get; set; }

        public bool Enabled { get; set; }
        public bool BasePreleaseOnFile
        {
            get => _basePreleaseOnFile ?? string.IsNullOrEmpty(TemplateFile);
            set => _basePreleaseOnFile = value;
        }

        public string GeneratedFilename { get; set; } = "{Application}-{NameSuffix}";
    }
}