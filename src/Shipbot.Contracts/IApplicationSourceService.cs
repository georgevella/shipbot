using System.Threading.Tasks;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.ApplicationSources
{
    public interface IApplicationSourceService
    {
        Task AddApplicationSource(Application application);
        Task StartDeploymentUpdateJob(DeploymentUpdate deploymentUpdate);
    }
}