using System.Collections.Generic;
using System.Threading.Tasks;
using Shipbot.Controller.Core.ApplicationSources.Models;
using Shipbot.Controller.Core.Configuration.ApplicationSources;
using Shipbot.Models;

namespace Shipbot.Controller.Core.ApplicationSources
{
    public interface IApplicationSourceService
    {
        Task AddApplicationSource(string applicationName, ApplicationSourceSettings applicationSourceSettings);
        Task StartDeploymentUpdateJob(DeploymentUpdate deploymentUpdate);
        Task<IEnumerable<ApplicationSource>> GetActiveApplications();
    }
}