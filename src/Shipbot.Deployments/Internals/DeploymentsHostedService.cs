using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Quartz;
using Shipbot.Deployments.Internals.Jobs;
using Shipbot.JobScheduling;

namespace Shipbot.Deployments.Internals
{
    public class DeploymentsHostedService : IHostedService
    {
        private readonly IScheduler _scheduler;

        public DeploymentsHostedService(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _scheduler.StartRecurringJob<ApplicationUpdatesPollingJob>("ApplicationUpdatePollingJob", "Deployments",
                TimeSpan.FromSeconds(2));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}