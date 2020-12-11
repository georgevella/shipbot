using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shipbot.ContainerRegistry.Models;
using Shipbot.Controller.Core.Configuration.Registry;

namespace Shipbot.ContainerRegistry.Dummy.Internals
{
    public class DummyRegistryClient : IRegistryClient
    {
        private readonly DummyRegistrySettings _settings;

        public DummyRegistryClient(DummyRegistrySettings settings)
        {
            _settings = settings;
        }

        public Task<bool> IsKnownRepository(string repository)
        {
            return Task.FromResult(_settings.Repositories.Keys.Contains(repository));
        }

        public Task<IEnumerable<ContainerImage>> GetRepositoryTags(string repository)
        {
            var containerImageRepository = _settings.Repositories[repository];

            return Task.FromResult(
                containerImageRepository.Images
                    .Select(x =>
                        new ContainerImage(
                            repository, 
                            x.Tag ?? Guid.NewGuid().ToString("D").Substring(0, 8), 
                            x.Hash ?? Guid.NewGuid().ToString("D"), 
                            x.CreationDateTime
                            )
                    )
                    .ToList()
                    .AsEnumerable()
            );
        }

        public Task<ContainerImage> GetImage(string repository, string tag)
        {
            throw new System.NotImplementedException();
        }
    }
}