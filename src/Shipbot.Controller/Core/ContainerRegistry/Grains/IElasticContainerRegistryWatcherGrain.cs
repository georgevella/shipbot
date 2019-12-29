using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.ContainerRegistry.Models;
using Shipbot.Controller.Core.ContainerRegistry.Watcher;

namespace Shipbot.Controller.Core.ContainerRegistry.Grains
{
    public interface IElasticContainerRegistryWatcherGrain : IGrainWithStringKey, IRemindable
    {
        Task Start();
        Task Stop();
        Task<IEnumerable<ImageTag>> GetTags();
    }
}