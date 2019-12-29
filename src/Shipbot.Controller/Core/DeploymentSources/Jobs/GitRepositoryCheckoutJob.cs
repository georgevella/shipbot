//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using Quartz;
//using Shipbot.Controller.Core.Apps;
//using Shipbot.Controller.Core.ContainerRegistry.Watcher;
//using Shipbot.Controller.Core.DeploymentSources.Models;
//using Shipbot.Controller.Core.Git.Models;
//using Shipbot.Controller.Core.Jobs;
//
//namespace Shipbot.Controller.Core.DeploymentSources.Jobs
//{
//    [DisallowConcurrentExecution]
//    public class GitRepositoryCheckoutJob : BaseScheduledJob<ApplicationSourceTrackingContext>
//    {
//        private readonly ILogger<GitRepositoryCheckoutJob> _log;
//        private readonly IApplicationSourceService _applicationSourceService;
//        private readonly IApplicationSourceSyncService _applicationSourceSyncService;
//        private readonly IApplicationService _applicationService;
//        private readonly IRegistryWatcher _registryWatcher;
//        private readonly IScheduler _scheduler;
//
//        public GitRepositoryCheckoutJob(
//            ILogger<GitRepositoryCheckoutJob> log,
//            IApplicationSourceService applicationSourceService,
//            IApplicationSourceSyncService applicationSourceSyncService,
//            IApplicationService applicationService,
//            IRegistryWatcher registryWatcher,
//            IScheduler scheduler)
//        {
//            _log = log;
//            _applicationSourceService = applicationSourceService;
//            _applicationSourceSyncService = applicationSourceSyncService;
//            _applicationService = applicationService;
//            _registryWatcher = registryWatcher;
//            _scheduler = scheduler;
//        }
//        
//        protected override async Task Execute(ApplicationSourceTrackingContext context)
//        {
//            var repository = context.Environment.Source.Repository;
//
//            // TODO: improve this to not have passwords in memory / use SecureStrings
//            var credentials = (UsernamePasswordGitCredentials) repository.Credentials;
//
//            await _applicationSourceService.CheckoutApplicationSource(
//                context.Environment.Source.Repository,
//                context.GitRepositoryPath);
//
//            var applicationSourceDetails = await _applicationSourceSyncService.BuildApplicationSourceDetails(context,
//                context.Environment.Source as HelmApplicationSource);
//            
//            foreach (var keyValuePair in applicationSourceDetails.Tags)
//            {
//                _applicationService.SetCurrentImageTag(
//                    context.Application,
//                    context.Environment, 
//                    keyValuePair.Key, 
//                    keyValuePair.Value
//                    );   
//            }
//
//            _log.LogInformation("Starting sync and repository watching jobs for {Application}",
//                context.Application
//            );
//            
//            await _scheduler.StartRecurringJob<GitRepositorySyncJob, ApplicationSourceTrackingContext>(
//                new JobKey($"gitwatch-{context.Application.Name}-{context.Environment.Name}", "gitrepowatcher"),
//                context, 10);
//            
//            await _registryWatcher.StartWatchingImageRepository(context.Application);
//
//
//
//
//            // start sync job
////            var jobData = jobExecutionContext.MergedJobDataMap;
////            var job = JobBuilder.Create<GitRepositorySyncJob>()
////                .WithIdentity($"gitwatch-{context.Application.Name}", "gitrepowatcher")
////                .UsingJobData(jobData)
////                .Build();
////
////            var trigger = TriggerBuilder.Create()
////                .WithIdentity($"gitwatch-trig-{context.Application.Name}", "gitrepowatcher")
////                .StartNow()
////                .WithSimpleSchedule(x => x
////                    .WithIntervalInSeconds(10)
////                    .RepeatForever()
////                )
////                .ForJob(job)
////                .Build();
//
//
//
////            await _scheduler.ScheduleJob(job, trigger);
//        }
//    }
//}