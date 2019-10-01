using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Quartz;
using Shipbot.Controller.Core.ApplicationSources.Jobs;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.ApplicationSources
{
    public class ApplicationSourceService : IApplicationSourceService
    {
        private readonly ConcurrentDictionary<string, ApplicationSourceTrackingContext> _trackingContexts =
            new ConcurrentDictionary<string, ApplicationSourceTrackingContext>();

        private readonly ILogger<ApplicationSourceService> _log;
        private readonly IScheduler _scheduler;

        public ApplicationSourceService(ILogger<ApplicationSourceService> log, IScheduler scheduler)
        {
            _log = log;
            _scheduler = scheduler;
        }
        
        public async Task AddApplicationSource(Application application)
        {            
            foreach (var keyValuePair in application.Environments)
            {
                var context = _trackingContexts.GetOrAdd(
                    $"{application.Name}__{keyValuePair.Value.Name}", 
                    (key, val) => new ApplicationSourceTrackingContext(application, val), 
                    keyValuePair.Value
                );

                var jobData = new JobDataMap((IDictionary<string, object>) new Dictionary<string, object>()
                {
                    {"Context", context}
                });
            
                var jobKey = new JobKey($"gitclone-{application.Name}", "gitrepowatcher");
            
                var job = JobBuilder.Create<GitRepositoryCheckoutJob>()
                    .WithIdentity(jobKey)
                    .UsingJobData(jobData)
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity($"gitclone-trig-{application.Name}", "gitrepowatcher")
                    .StartNow()
                    .ForJob(job)
                    .Build();

                await _scheduler.ScheduleJob(job, trigger);
            }
        }

        public async Task CheckoutApplicationSource(
            ApplicationSourceRepository applicationSourceRepository, 
            DirectoryInfo checkoutDirectory)
        {

            if (checkoutDirectory.Exists)
            {
                checkoutDirectory.Delete(true);
                checkoutDirectory.Create();
            }
            
            await CheckoutSources(applicationSourceRepository.Uri, applicationSourceRepository.Ref, checkoutDirectory,
                applicationSourceRepository.Credentials);
        }

        private async Task CheckoutSources(Uri repository, string branch, DirectoryInfo checkoutDirectory, GitCredentials credentials = null )
        {
            _log.LogInformation("Cloning {Repository} into {Path}",
                repository,
                checkoutDirectory.FullName);
            
            var options = new CloneOptions();
            if (credentials is UsernamePasswordGitCredentials usernamePasswordGitCredentials)
            {
                options.CredentialsProvider = (url, fromUrl, types) => new UsernamePasswordCredentials()
                {
                    Username = usernamePasswordGitCredentials.Username,
                    Password = usernamePasswordGitCredentials.Password
                };
            }

            options.Checkout = true;
            options.BranchName = branch;
            
            await Task.Run(() =>
            {
                LibGit2Sharp.Repository.Clone(
                    repository.ToString(),
                    checkoutDirectory.FullName,
                    options
                    );
            });
        }
    }

    public interface IApplicationSourceService
    {
        Task AddApplicationSource(Application application);

        Task CheckoutApplicationSource(
            ApplicationSourceRepository applicationSourceRepository,
            DirectoryInfo checkoutDirectory);
    }
}