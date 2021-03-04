using System;
using Shipbot.Models;

namespace Shipbot.Controller.Core.ApplicationSources.Models
{
    /// <summary>
    ///     Git repository and branch from where the deployment manifest is loaded.
    /// </summary>
    public class DeploymentManifestSource
    {
        /// <summary>
        ///     Address of git repository containing deployment manifest.
        /// </summary>
        public Uri Uri { get; set; }
        
        public string Ref { get; set; }
        
        public GitCredentials Credentials { get; set; }
    }
}