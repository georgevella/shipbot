using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shipbot.Applications;
using Shipbot.Deployments.Models;

namespace Shipbot.Deployments
{
    public class DeploymentWorkflowService : IDeploymentWorkflowService
    {
        private readonly ILogger<DeploymentWorkflowService> _log;
        private readonly IApplicationService _applicationService;
        private readonly IDeploymentService _deploymentService;
        private readonly IDeploymentQueueService _deploymentQueueService;

        public DeploymentWorkflowService(
            ILogger<DeploymentWorkflowService> log,
            IApplicationService applicationService,
            IDeploymentService deploymentService,
            IDeploymentQueueService deploymentQueueService
            )
        {
            _log = log;
            _applicationService = applicationService;
            _deploymentService = deploymentService;
            _deploymentQueueService = deploymentQueueService;
        }
        
        public async Task<IEnumerable<Deployment>> StartImageDeployment(
            string containerRepository, 
            string tag
        )
        {
            var applications = _applicationService.GetApplications();
            var allApplicationsTrackingThisRepository = applications
                .SelectMany(
                    x => x.Images,
                    (app, img) =>
                        new
                        {
                            Image = img,
                            Application = app
                        }
                )
                .Where(x =>
                    x.Image.Repository.Equals(containerRepository) &&
                    x.Image.Policy.IsMatch(tag)
                );

            var createdDeployments = new List<Deployment>();

            foreach (var item in allApplicationsTrackingThisRepository)
            {
                try
                {
                    var deployment = await _deploymentService.AddDeployment(item.Application, item.Image, tag);

                    if (item.Application.AutoDeploy)
                    {
                        _log.LogDebug("Adding deployment to deployment queue.");
                        // push deployment onto the queue since we have AutoDeploy set
                        await _deploymentQueueService.EnqueueDeployment(deployment);
                    }
                    
                    createdDeployments.Add(deployment);
                }
                catch
                {
                    // ignored
                }
            }

            return createdDeployments;
        }

    }

    /// <summary>
    ///     Dictates the flow of actions taken when a new deployment is triggered
    /// </summary>
    public interface IDeploymentWorkflowService
    {
        Task<IEnumerable<Deployment>> StartImageDeployment(
            string containerRepository, 
            string tag
        );
    }
}