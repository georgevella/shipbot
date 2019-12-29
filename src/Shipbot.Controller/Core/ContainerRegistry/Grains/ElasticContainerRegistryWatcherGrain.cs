using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Streams;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.ContainerRegistry.GrainState;
using Shipbot.Controller.Core.ContainerRegistry.Models;
using Shipbot.Controller.Core.ContainerRegistry.Watcher;
using Shipbot.Controller.Core.Utilities;

namespace Shipbot.Controller.Core.ContainerRegistry.Grains
{
    [StorageProvider(ProviderName = ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME)]
    public class ElasticContainerRegistryWatcherGrain : Grain<RegistryWatcherGrainState>, IElasticContainerRegistryWatcherGrain
    {
        private const string RefreshReminder = "RefreshReminder";
        
        private readonly ILogger<ElasticContainerRegistryWatcherGrain> _log;
        private readonly RegistryClientPool _registryClientPool;
        private IAsyncStream<ImageTag> _stream;

        public ElasticContainerRegistryWatcherGrain(
            ILogger<ElasticContainerRegistryWatcherGrain> log,
            RegistryClientPool registryClientPool
            )
        {
            _log = log;
            _registryClientPool = registryClientPool;
        }

        public override async Task OnActivateAsync()
        {
            var imageRepository = this.GetPrimaryKeyString();

            if (State.Tags.Count == 0)
            {
                // populate the state with all tags
                var tags = await FetchTags();
                tags.ForEach( t => State.Tags.Add(t));
            }
            
            var streamProvider = GetStreamProvider(Constants.InternalMessageStreamProvider);

            _stream = streamProvider.GetStream<ImageTag>(imageRepository.CreateGuidFromString(),
                Constants.ContainerRegistryQueueNamespace);

            await WriteStateAsync();

            await base.OnActivateAsync();
        }

        public async Task Start()
        {
            var reminders = await GetReminders();
            if (reminders.All(x => x.ReminderName != RefreshReminder))
            {
                await RegisterOrUpdateReminder(RefreshReminder, TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1));
            }
        }

        public async Task Stop()
        {
            var reminder = await GetReminder(RefreshReminder);

            await UnregisterReminder(reminder);
        }

        public Task<IEnumerable<ImageTag>> GetTags()
        {
            return Task.FromResult(State.Tags.ToArray().AsEnumerable());
        }

        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            var imageRepository = this.GetPrimaryKeyString();
            
            var tags = await FetchTags();
            
            // ensure we keep a record of new tags
            var newTags = tags
                .Except(State.Tags, ImageTag.EqualityComparer)
                .ToList();

            if (newTags.Any())
            {
                _log.LogInformation("Found {NewTagCount} tags for repository {Repository}", newTags.Count, imageRepository);
            
                foreach (var imageTag in newTags)
                {
                    State.Tags.Add(imageTag);
                }

                await WriteStateAsync();
            
                // push all new image tags onto the queue
                newTags.ForEach( async imageTag => await _stream.OnNextAsync(imageTag));    
            }
        }
        
        /// <summary>
        ///     Retrieves all tags from the ECR Client
        /// </summary>
        /// <returns></returns>
        private async Task<List<ImageTag>> FetchTags()
        {
            var imageRepository = this.GetPrimaryKeyString();
            
            var client = await _registryClientPool.GetRegistryClientForRepository(imageRepository);

            var tags = (await client.GetRepositoryTags(imageRepository))
                .Select(
                    (tuple, i) =>
                        new ImageTag(imageRepository, tuple.tag, tuple.createdAt)
                )
                .ToList();
            
            return tags;
        }
    }
}