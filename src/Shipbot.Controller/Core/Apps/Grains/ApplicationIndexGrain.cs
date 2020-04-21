using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using Shipbot.Controller.Core.Apps.GrainState;
using Shipbot.Controller.Core.Apps.Models;

namespace Shipbot.Controller.Core.Apps.Grains
{
    [StorageProvider(ProviderName = ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME)]
    public class ApplicationIndexGrain : Grain<ApplicationIndex>, IApplicationIndexGrain
    {
        public Task EnsureRegistered(ApplicationKey applicationKey)
        {
            State.Applications.Add(applicationKey);

            return WriteStateAsync();
        }

        public Task<IReadOnlyCollection<ApplicationKey>> GetAllApplications()
        {
            return Task.FromResult( (IReadOnlyCollection<ApplicationKey>)State.Applications.ToList().AsReadOnly());
        }
    }

    public interface IApplicationIndexGrain : IGrain, IGrainWithGuidKey
    {
        Task EnsureRegistered(ApplicationKey applicationKey);
        Task<IReadOnlyCollection<ApplicationKey>> GetAllApplications();
    }
}