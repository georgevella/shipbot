using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shipbot.Controller.Core.ContainerRegistry.Clients
{
    public class RegistryClientPool
    {
        private readonly List<IRegistryClient> _registryClients = new List<IRegistryClient>();

        public async Task<IRegistryClient> GetRegistryClientForRepository(string repository)
        {
            foreach (var c in _registryClients)
            {
                var known = await c.IsKnownRepository(repository);
                if (known)
                    return c;
            }

            return null;
        }

        public void AddClient(IRegistryClient client)
        {
            _registryClients.Add(client);
        }
    }
}