using System.Threading.Tasks;
using Shipbot.Models;

namespace Shipbot.Contracts
{
    public interface IApplicationSourceService
    {
        Task AddApplicationSource(Application application);
        Task StartDeploymentUpdateJob(DeploymentUpdate deploymentUpdate);
    }
}