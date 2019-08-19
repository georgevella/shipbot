using System.IO;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Shipbot.Controller.Core.ApplicationSources
{
    [DisallowConcurrentExecution]
    public class GitRepositoryCheckoutJob : IJob
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
        
        public async Task Execute(IJobExecutionContext jobExecutionContext)
        {
            var data = jobExecutionContext.JobDetail.JobDataMap;
            var context = (ApplicationSourceTrackingContext) data["Context"];

            var repository = context.Application.Source.Repository;

            // TODO: improve this to not have passwords in memory / use SecureStrings
            var credentials = (UsernamePasswordGitCredentials) repository.Credentials;

            if (Directory.Exists(context.GitRepositoryPath))
            {
//                if (!Directory.Exists(Path.Combine(context.GitRepositoryPath, ".git/")))
//                {
//                    
//                }
                Directory.Delete(context.GitRepositoryPath, true);
            }

            _log.LogInformation("Cloning {repository} into {path}",
                repository.Uri,
                context.GitRepositoryPath);

            await Task.Run(() =>
            {
                LibGit2Sharp.Repository.Clone(
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
            });

            _log.LogInformation("Starting sync-job for {repository} in {path}",
                repository.Uri,
                context.GitRepositoryPath
            );
            
            // start sync job
            var jobData = jobExecutionContext.MergedJobDataMap;
            var job = JobBuilder.Create<GitRepositorySyncJob>()
                .WithIdentity($"gitwatch-{context.Application.Name}", "gitrepowatcher")
                .UsingJobData(jobData)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"gitwatch-trig-{context.Application.Name}", "gitrepowatcher")
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