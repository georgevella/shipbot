using System.Collections.Generic;
using System.Threading.Tasks;
using Shipbot.Controller.Core.ApplicationSources.Models;
using Shipbot.Controller.Core.Configuration.DeploymentManifests;
using Shipbot.Models;

namespace Shipbot.Controller.Core.ApplicationSources
{
    public interface IDeploymentManifestSourceService
    {
        /// <summary>
        ///     Add a deployment source for an application.
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="deploymentManifestSourceSettings"></param>
        /// <returns></returns>
        Task Add(string applicationName, DeploymentManifestSourceSettings deploymentManifestSourceSettings);
        Task StartDeploymentUpdateJob(DeploymentUpdate deploymentUpdate);
        Task<IEnumerable<DeploymentManifest>> GetActiveApplications();
    }
}