namespace Shipbot.Controller.Core.ApplicationSources.Models
{
    public abstract class DeploymentManifest
    {
        protected DeploymentManifest(string application, DeploymentManifestSource repository, string path)
        {
            Application = application;
            Repository = repository;
            Path = path;
        }

        public string Application { get; }
        public DeploymentManifestSource Repository { get;  }
        
        public string Path { get; }
    }
}