using System.Threading.Tasks;
using Shipbot.Models;

namespace Shipbot.SlackIntegration
{
    public interface IDeploymentNotificationService
    {
        Task CreateNotification(DeploymentUpdate deploymentUpdate);
        Task UpdateNotification(DeploymentUpdate deploymentUpdate, DeploymentUpdateStatus status);
    }
}