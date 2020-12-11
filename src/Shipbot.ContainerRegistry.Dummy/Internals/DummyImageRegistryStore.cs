using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shipbot.ContainerRegistry.Models;
using Shipbot.Controller.Core.Configuration.Registry;

namespace Shipbot.ContainerRegistry.Dummy.Internals
{
    internal class DummyImageRegistryStore : IDummyImageRegistryStore
    {
        private readonly IOptions<DummyRegistrySettings> _dummyRegistrySettings;

        public static readonly ConcurrentDictionary<string, ConcurrentBag<ContainerImage>> Store =
            new ConcurrentDictionary<string, ConcurrentBag<ContainerImage>>();


        public DummyImageRegistryStore(IOptions<DummyRegistrySettings> dummyRegistrySettings)
        {
            _dummyRegistrySettings = dummyRegistrySettings;

            foreach (var pair in _dummyRegistrySettings.Value.Repositories)
            {
                pair.Value.Images
                    .Select(x =>
                        new ContainerImage(
                            pair.Key,
                            x.Tag,
                            x.Hash,
                            x.CreationDateTime
                        )
                    )
                    .ToList()
                    .ForEach( x => AddContainerImage(x));
            }
        }
        
        public Task AddContainerImage(ContainerImage containerImage)
        {
            var bag = Store.GetOrAdd(containerImage.Repository, registry => new ConcurrentBag<ContainerImage>());
            bag.Add(containerImage);
            
            return Task.CompletedTask;
        }
    }
}