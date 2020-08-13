using System.Threading.Tasks;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Deployments
{
    public interface IDeploymentNotificationService
    {
        Task CreateNotification(DeploymentUpdate deploymentUpdate);
        Task UpdateNotification(DeploymentUpdate deploymentUpdate, DeploymentUpdateStatus status);
    }
}