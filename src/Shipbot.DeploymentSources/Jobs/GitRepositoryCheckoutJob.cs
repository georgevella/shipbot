using System.IO;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Quartz;
using Shipbot.Controller.Core.ApplicationSources.Models;
using Shipbot.JobScheduling;
using Shipbot.Models;

namespace Shipbot.Controller.Core.ApplicationSources.Jobs
{
    [DisallowConcurrentExecution]
    public class GitRepositoryCheckoutJob : BaseJobWithData<DeploymentManifestSourceTrackingContext>
    {
        private readonly ILogger<GitRepositoryCheckoutJob> _log;
        private readonly IScheduler _scheduler;

        public GitRepositoryCheckoutJob(
            ILogger<GitRepositoryCheckoutJob> log,
            IScheduler scheduler)
        {
            _log = log;
            _scheduler = scheduler;
        }

        public override async Task Execute(DeploymentManifestSourceTrackingContext context)
        {
            var repository = context.DeploymentManifest.Repository;
            
            _log.LogInformation("Checkout repository containing application declaration");
            var credentials = (UsernamePasswordGitCredentials) repository.Credentials;

            if (Directory.Exists(context.GitRepositoryPath))
            {
                _log.LogInformation("Removing local copy of git repository",
                    repository.Uri,
                    context.GitRepositoryPath);
                Directory.Delete(context.GitRepositoryPath, true);
            }

            _log.LogInformation("Cloning {Repository} into {Path}",
                repository.Uri,
                context.GitRepositoryPath);

            Repository.Clone(
                repository.Uri.ToString(),
                context.GitRepositoryPath,
                new CloneOptions()
                {
                    CredentialsProvider = (url, fromUrl, types) => new UsernamePasswordCredentials()
                    {
                        Username = credentials.Username,
                        Password = credentials.Password
                    }
                });

            _log.LogInformation("Starting sync-job for {Repository} in {Path}",
                repository.Uri,
                context.GitRepositoryPath
            );

            var job = JobFactory.BuildJobWithData<GitRepositorySyncJob, DeploymentManifestSourceTrackingContext>(
                $"gitwatch-{context.ApplicationName}", 
                Constants.SchedulerGroup, 
                context
                );

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"gitwatch-trig-{context.ApplicationName}", Constants.SchedulerGroup)
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(10)
                    .RepeatForever()
                )
                .ForJob(job)
                .Build();

            await _scheduler.ScheduleJob(job, trigger);
        }
    }
}