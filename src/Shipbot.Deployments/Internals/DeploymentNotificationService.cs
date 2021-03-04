using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Octokit;
using Shipbot.Applications;
using Shipbot.Data;
using Shipbot.Deployments.Models;
using Shipbot.SlackIntegration;
using Shipbot.SlackIntegration.Internal;
using Deployment = Shipbot.Deployments.Models.Deployment;
using DeploymentNoficationDao = Shipbot.Deployments.Dao.DeploymentNotification;

namespace Shipbot.Deployments
{
    public class DeploymentNotificationService : IDeploymentNotificationService
    {
        private readonly ILogger<DeploymentNotificationService> _log;
        private readonly IDeploymentNotificationBuilder _deploymentNotificationBuilder;
        private readonly IApplicationService _applicationService;
        private readonly IEntityRepository<DeploymentNoficationDao> _deploymentNotificationRepository;
        private readonly ISlackClient _slackClient;
        private readonly IGitHubClient _gitHubClient;

        public DeploymentNotificationService(
            ILogger<DeploymentNotificationService> log,
            IDeploymentNotificationBuilder deploymentNotificationBuilder,
            IApplicationService applicationService,
            IEntityRepository<DeploymentNoficationDao> deploymentNotificationRepository,
            ISlackClient slackClient,
            IGitHubClient gitHubClient
        )
        {
            _log = log;
            _deploymentNotificationBuilder = deploymentNotificationBuilder;
            _applicationService = applicationService;
            _deploymentNotificationRepository = deploymentNotificationRepository;
            _slackClient = slackClient;
            _gitHubClient = gitHubClient;
        }
        
        public async Task<bool> CreateNotification(Deployment deployment)
        {
            var application = _applicationService.GetApplication(deployment.ApplicationId);
            var channel = application.Notifications.Channels.FirstOrDefault();

            var applicationImage = application.Images.FirstOrDefault(
                x => x.Repository == deployment.ImageRepository &&
                     x.TagProperty.Path == deployment.UpdatePath
            );
            
            if (applicationImage == null)
            {
                _log.LogCritical("Received a deployment for '{Image}' on '{TagPath}' but we could not find it in the application manifest",
                    deployment.ImageRepository,
                    deployment.UpdatePath
                    );

                return false;
            }
            
            if (channel != null)
            {
                _log.LogInformation(
                    "Sending notification about image tag update operation for '{Repository}' with {Tag} for application {Application} with new tag {NewTag}",
                    deployment.ImageRepository,
                    deployment.CurrentTag,
                    deployment.ApplicationId,
                    deployment.TargetTag
                );
                try
                {
                    var notification =
                        _deploymentNotificationBuilder.BuildNotification(
                            deployment
                            );
                    
                    // send notification to slack
                    var handle = await _slackClient.PostMessageAsync(channel, notification);
                    
                    await _deploymentNotificationRepository.Add(new DeploymentNoficationDao()
                    {
                        Id = Guid.NewGuid(),
                        DeploymentId = deployment.Id,
                        SlackMessageId = handle.Id
                    });

                    await _deploymentNotificationRepository.Save();
                }
                catch (Exception e)
                {
                    _log.LogError(e, "Failed to send deployment update notification to slack");
                    return false;
                }
            }
            
            // check if this is a preview release and send a notification to the github pr
            if (deployment.Type == DeploymentType.PreviewRelease)
            {
                if (applicationImage.SourceCode.IsAvailable)
                {
                    var pullRequests = await _gitHubClient.PullRequest.GetAllForRepository(
                        "River-iGaming",
                        "mangata.betti",
                        new PullRequestRequest()
                        {
                            State = ItemStateFilter.Open
                        });

                    var pullRequestMap = pullRequests.Select(x => (
                                prObj: x,
                                branch: x.Head.Label,
                                number: x.Number,
                                isOpen: x.State.Value == ItemState.Open,
                                id: x.Id,
                                title: x.Title,
                                creator: x.User.Email ?? x.User.Name
                            )
                        )
                        .ToList();

                    deployment.Parameters.TryGetValue(DeploymentParameterConstants.PreviewReleaseBranch, out var branch);

                    var matchingPullrequests = pullRequestMap
                        .Where(x => x.branch.Contains(branch, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (matchingPullrequests.Count != 1)
                    {
                        _log.LogCritical(
                            "Multiple, or no open pull requests or branches with the base name ('{Branch}') were detected (count: {Count})",
                            branch,
                            matchingPullrequests.Count);
                    }
                    else
                    {
                        var relevantPullRequest = matchingPullrequests[0];

                        if (deployment.CurrentTag == string.Empty)
                        {
                            var message = new StringBuilder(
                                "We have detected a new container image that matches this pull request.  A new preview release is being created and deployed."
                            );

                            if (deployment.Parameters.TryGetValue(DeploymentParameterConstants.Hostname,
                                out var hostname))
                            {
                                message.Append(
                                    $"It will be available at https://{hostname} shortly."
                                );
                            }

                            _log.LogTrace("Submitting new preview release deployment to github pr");
                            // we are created a new preview release, let's notify the devs on the PR.
                            var result = await _gitHubClient.Issue.Comment.Create(
                                applicationImage.SourceCode.Github.Owner,
                                applicationImage.SourceCode.Github.Repository,
                                relevantPullRequest.number,
                                message.ToString()
                            );
                        }
                        else
                        {
                            _log.LogTrace("Submitting preview release deployment update to github pr");
                            // we are created a new preview release, let's notify the devs on the PR.
                            // var result = await _gitHubClient.Issue.Comment.Create(
                            //     applicationImage.SourceCode.Github.Owner,
                            //     applicationImage.SourceCode.Github.Repository,
                            //     relevantPullRequest.number,
                            //     "A new image was detected for this preview release. A new deployment is underway and will be available shortly."
                            // );
                        }
                    }
                }
                else
                {
                    _log.LogError("Could not set deployment notification to Github Pull Request");
                }
            }
            
            return true;
        }

        public async Task UpdateNotification(Deployment deployment)
        {
            if (deployment == null) 
                throw new ArgumentNullException(nameof(deployment));
            
            var deploymentNotificationDao = await _deploymentNotificationRepository.Query()
                .FirstOrDefaultAsync(x => x.DeploymentId == deployment.Id);
            if (deploymentNotificationDao != null)
            {
                var handle = new MessageHandle(deploymentNotificationDao.SlackMessageId);

                try
                {
                    _log.LogInformation("Submitting {@DeploymentUpdate} notification change to slack {@MessageHandle}. ", deployment, handle);
                    var notification = _deploymentNotificationBuilder.BuildNotification(deployment);
                    var newHandle = await _slackClient.UpdateMessageAsync(handle, notification);
                    // NotificationHandles.TryUpdate(deployment.Id, newHandle, handle);
                }
                catch (Exception e)
                {
                    _log.LogError(e, "Failed to submit {@DeploymentUpdate} notification {@MessageHandle}", deployment, handle);
                }    
            }
            else
            {
                _log.LogCritical("We received an UpdateNotification operation for deployment {DeploymentId} of {Image}:{Tag} but didn't find it in Db",
                    deployment.Id,
                    deployment.ImageRepository,
                    deployment.TargetTag);
            }
        }
    }
    
    public interface IDeploymentNotificationService
    {
        Task<bool> CreateNotification(Deployment deployment);
        Task UpdateNotification(Deployment deployment);
    }
}