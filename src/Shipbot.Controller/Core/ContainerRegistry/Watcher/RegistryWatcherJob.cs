//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using Quartz;
//using Shipbot.Controller.Core.ContainerRegistry.Models;
//using Shipbot.Controller.Core.Deployments;
//
//namespace Shipbot.Controller.Core.ContainerRegistry.Watcher
//{
//    [DisallowConcurrentExecution]
//    class RegistryWatcherJob : IJob
//    {
//        private readonly ILogger<RegistryWatcherJob> _log;
//        private readonly RegistryClientPool _registryClientPool;
//        private readonly IDeploymentService _deploymentService;
//        private readonly IRegistryWatcherStorage _storage;
//
//        public RegistryWatcherJob(
//            ILogger<RegistryWatcherJob> log,
//            RegistryClientPool registryClientPool,
//            IDeploymentService deploymentService,
//            IRegistryWatcherStorage storage
//        )
//        {
//            _log = log;
//            _registryClientPool = registryClientPool;
//            _deploymentService = deploymentService;
//            _storage = storage;
//        }
//            
//        public async Task Execute(IJobExecutionContext context)
//        {
//            var dataMap = context.MergedJobDataMap;
//
//            var imageRepository = (string) dataMap["ImageRepository"];
//
//            using (_log.BeginScope(new Dictionary<string, object>()
//            {
//                {"Repository", imageRepository}
//            }))
//            {
//                _log.LogInformation("Fetching tags for {imagerepository}", imageRepository);
//                var client = await _registryClientPool.GetRegistryClientForRepository(imageRepository);
//
//                var tags = await client.GetRepositoryTags(imageRepository);
//
//                var newTags = _storage.AddOrUpdateImageTags(imageRepository,
//                    tags.Select((tuple, i) => new ImageTag(imageRepository, tuple.tag, tuple.createdAt))
//                    );
//
//
//                context.Result = newTags;
//            }
//
//        }
//    }
//}