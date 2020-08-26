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
        Task ChangeDeploymentUpdateStatus(Guid deploymentId, DeploymentStatus status);

        Task FinishDeploymentUpdate(
            Guid deploymentId,
            DeploymentStatus finalStatus
        );

        Task<IEnumerable<Deployment>> GetDeployments(Application? application, DeploymentStatus? status);
        Task<Deployment> GetDeployment(Guid deploymentId);
    }
}