using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
using Shipbot.Controller.Core.Configuration.ApplicationSources;
using Shipbot.JobScheduling;
using Shipbot.Models;
using ApplicationSourceRepository = Shipbot.Controller.Core.ApplicationSources.Models.ApplicationSourceRepository;

namespace Shipbot.Controller.Core.ApplicationSources
{
    public class ApplicationSourceService : IApplicationSourceService
    {
        // private readonly ConcurrentDictionary<string, ApplicationSourceTrackingContext> _trackingContexts =
        //     new ConcurrentDictionary<string, ApplicationSourceTrackingContext>();

        private readonly ILogger<ApplicationSourceService> _log;
        private readonly IOptions<ShipbotConfiguration> _configuration;
        private readonly IScheduler _scheduler;

        public ApplicationSourceService(
            ILogger<ApplicationSourceService> log, 
            IOptions<ShipbotConfiguration> configuration,
            IScheduler scheduler
            )
        {
            _log = log;
            _configuration = configuration;
            _scheduler = scheduler;
        }
        
        public async Task AddApplicationSource(string applicationName, ApplicationSourceSettings applicationSourceSettings)
        {
            if (await _scheduler.CheckExists(new JobKey($"gitclone-{applicationName}", Constants.SchedulerGroup)))
            {
                throw new Exception();
            }
            
            var conf = _configuration.Value;

            var applicationSource = applicationSourceSettings.Type switch {
                ApplicationSourceType.Helm => (ApplicationSource) new HelmApplicationSource(
                    applicationName,
                    new ApplicationSourceRepository()
                    {
                        // TODO: handle config changes
                        Credentials = conf.GitCredentials.First(
                            x =>
                                x.Name.Equals(applicationSourceSettings.Repository.Credentials)
                        ).ConvertToGitCredentials(),
                        Ref = applicationSourceSettings.Repository.Ref,
                        Uri = new Uri(applicationSourceSettings.Repository.Uri)
                    },
                    applicationSourceSettings.Path,
                    applicationSourceSettings.Helm.ValueFiles,
                    applicationSourceSettings.Helm.Secrets
                ),
                _ => throw new InvalidOperationException() 
            };
            
            var context = new ApplicationSourceTrackingContext(applicationName, applicationSource);

            await _scheduler.TriggerJobOnce<GitRepositoryCheckoutJob, ApplicationSourceTrackingContext>(
                $"gitclone-{applicationName}",
                Constants.SchedulerGroup,
                context
            );
        }

        public async Task<IEnumerable<ApplicationSource>> GetActiveApplications()
        {
            var jobKeys = await _scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(Constants.SchedulerGroup));

            var jobDetails = new List<IJobDetail>();
            foreach (var key in jobKeys)
            {
                var detail = await _scheduler.GetJobDetail(key);
                if (detail != null)
                    jobDetails.Add(detail);
            }

            var trackingContexts = jobDetails.Select(x => x.JobDataMap.GetJobData<ApplicationSourceTrackingContext>()).ToList();

            return trackingContexts.Select(x => x.ApplicationSource).ToList();
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