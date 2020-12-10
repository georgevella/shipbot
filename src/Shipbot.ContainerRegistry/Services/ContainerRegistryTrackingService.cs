using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Shipbot.ContainerRegistry.Watcher;
using Shipbot.Contracts;
using Shipbot.JobScheduling;
using Shipbot.Models;

namespace Shipbot.ContainerRegistry.Services
{
    public class ContainerRegistryTrackingService : IRegistryWatcher
    {
        private readonly ILogger<ContainerRegistryTrackingService> _log;
        private readonly IScheduler _scheduler;
        private readonly HashSet<string> _jobs = new HashSet<string>();
        
        private const string  PollingJobGroup = "containerrepowatcher";

        public ContainerRegistryTrackingService(ILogger<ContainerRegistryTrackingService> log, IScheduler scheduler)
        {
            _log = log;
            _scheduler = scheduler;
        }
        public async Task StartWatchingImageRepository(string containerImageRepository)
        {
            var jobKey = GenerateJobKey(containerImageRepository);

            if (_jobs.Add(jobKey))
            {
                _log.LogInformation($"Adding job to track container repository '{containerImageRepository}'.");
                await _scheduler.StartRecurringJob<ContainerRegistryPollingJob, ContainerRepositoryPollingContext>(
                    jobKey, 
                    PollingJobGroup, 
                    new ContainerRepositoryPollingContext(containerImageRepository), 
                    TimeSpan.FromSeconds(10) 
                );
            }
            else
            {
                _log.LogWarning($"We are already tracking '{containerImageRepository}', job not added.");
            }
        }

        public Task<bool> IsWatched(string containerImageRepository)
        {
            var jobKey = GenerateJobKey(containerImageRepository);
            return Task.FromResult(_jobs.Contains(jobKey));
        }

        private static string GenerateJobKey(string containerRepository)
        {
            var jobKey = $"rwatcher-{containerRepository.Replace('/', '-')}";
            return jobKey;
        }

        public async Task StopWatchingImageRepository(string containerImageRepository)
        {
            var jobKey = GenerateJobKey(containerImageRepository);
            if (_jobs.Remove(jobKey))
            {
                await _scheduler.StopRecurringJob(jobKey, PollingJobGroup);
            }
        }

        public async Task Shutdown()
        {
            foreach (var jobKey in _jobs.Where(jobKey => jobKey!=null))
            {
                await _scheduler.StopRecurringJob(jobKey, PollingJobGroup);
            }
        }
    }
}