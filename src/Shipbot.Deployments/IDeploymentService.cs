using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shipbot.Deployments.Models;
using Shipbot.Models;

namespace Shipbot.Deployments
{
    public interface IDeploymentService
    {
        Task<Deployment> AddDeployment(Application application, Image image, string newTag);
        Task ChangeDeploymentUpdateStatus(DeploymentUpdate deploymentUpdate, DeploymentUpdateStatus status);

        Task FinishDeploymentUpdate(
            DeploymentUpdate deploymentUpdate,
            DeploymentUpdateStatus finalStatus
        );

        Task<IEnumerable<Deployment>> GetDeployments(Application application);
        Task<Deployment> GetDeployment(Guid deploymentId);
    }
}