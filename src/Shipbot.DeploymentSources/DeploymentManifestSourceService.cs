using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Simpl;
using Shipbot.Contracts;
using Shipbot.Controller.Core.ApplicationSources.Jobs;
using Shipbot.Controller.Core.ApplicationSources.Models;
using Shipbot.Controller.Core.Configuration;
using Shipbot.Controller.Core.Configuration.DeploymentManifests;
using Shipbot.JobScheduling;
using Shipbot.Models;

namespace Shipbot.Controller.Core.ApplicationSources
{
    public class DeploymentManifestSourceService : IDeploymentManifestSourceService
    {
        private readonly ILogger<DeploymentManifestSourceService> _log;
        private readonly IOptions<ShipbotConfiguration> _configuration;
        private readonly IScheduler _scheduler;

        public DeploymentManifestSourceService(
            ILogger<DeploymentManifestSourceService> log, 
            IOptions<ShipbotConfiguration> configuration,
            IScheduler scheduler
            )
        {
            _log = log;
            _configuration = configuration;
            _scheduler = scheduler;
        }
        
        public async Task Add(string applicationName, DeploymentManifestSourceSettings deploymentManifestSourceSettings)
        {
            var jobKey = new JobKey($"gitclone-{applicationName}", Constants.SchedulerGroup);
            if (await _scheduler.CheckExists(jobKey))
            {
                var triggers = await _scheduler.GetTriggersOfJob(jobKey);
                foreach (var trigger in triggers)
                {
                    await _scheduler.UnscheduleJob(trigger.Key);
                }
                await _scheduler.DeleteJob(new JobKey($"gitclone-{applicationName}", Constants.SchedulerGroup));
            }
            
            var conf = _configuration.Value;
            var applicationSource = deploymentManifestSourceSettings.Type switch {
                DeploymentManifestType.Helm => (DeploymentManifest) new HelmDeploymentManifest(
                    applicationName,
                    new DeploymentManifestSource()
                    {
                        // TODO: handle config changes
                        Credentials = conf.GitCredentials.First(
                            x =>
                                x.Name.Equals(deploymentManifestSourceSettings.Repository.Credentials)
                        ).ConvertToGitCredentials(),
                        Ref = deploymentManifestSourceSettings.Repository.Ref,
                        Uri = new Uri(deploymentManifestSourceSettings.Repository.Uri)
                    },
                    deploymentManifestSourceSettings.Path,
                    deploymentManifestSourceSettings.Helm.ValueFiles,
                    deploymentManifestSourceSettings.Helm.Secrets
                ),
                DeploymentManifestType.Raw => (DeploymentManifest) new RawDeploymentManifest(
                    applicationName,
                    new DeploymentManifestSource()
                    {
                        // TODO: handle config changes
                        Credentials = conf.GitCredentials.First(
                            x =>
                                x.Name.Equals(deploymentManifestSourceSettings.Repository.Credentials)
                        ).ConvertToGitCredentials(),
                        Ref = deploymentManifestSourceSettings.Repository.Ref,
                        Uri = new Uri(deploymentManifestSourceSettings.Repository.Uri)
                    },
                    deploymentManifestSourceSettings.Path,
                    new [] { deploymentManifestSourceSettings.Raw.File },
                    deploymentManifestSourceSettings.Raw.PreviewRelease
                    ),
                _ => throw new InvalidOperationException() 
            };
            
            var context = new DeploymentManifestSourceTrackingContext(
                applicationName, 
                applicationSource,
                Path.Combine(Path.GetTempPath(), $"sb-{Guid.NewGuid()}-{applicationName}-{applicationSource.Repository.Ref}")
                );

            await _scheduler.TriggerJobOnce<GitRepositoryCheckoutJob, DeploymentManifestSourceTrackingContext>(
                $"gitclone-{applicationName}",
                Constants.SchedulerGroup,
                context
            );
        }

        public async Task<IEnumerable<DeploymentManifest>> GetActiveApplications()
        {
            var jobKeys = await _scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(Constants.SchedulerGroup));
            var jobDetails = new List<IJobDetail>();
            foreach (var key in jobKeys)
            {
                var detail = await _scheduler.GetJobDetail(key);
                if (detail != null)
                    jobDetails.Add(detail);
            }

            var trackingContexts = jobDetails.Select(x => x.JobDataMap.GetJobData<DeploymentManifestSourceTrackingContext>()).ToList();
            return trackingContexts.Select(x => x.DeploymentManifest).ToList();
        }

        public async Task StartDeploymentUpdateJob(DeploymentUpdate deploymentUpdate)
        {
            var jobkey = new JobKey($"gitwatch-{deploymentUpdate.Application.Name}", Constants.SchedulerGroup);

            var data = new JobDataMap
            {
                ["DeploymentUpdate"] = deploymentUpdate
            };

            await _scheduler.TriggerJob(jobkey, data, CancellationToken.None);
        }
    }
}