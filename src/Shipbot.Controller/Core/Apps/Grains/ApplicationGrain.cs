using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Providers;
using Shipbot.Controller.Core.Apps.GrainState;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Configuration.ApplicationSources;
using Shipbot.Controller.Core.Configuration.Apps;
using Shipbot.Controller.Core.DeploymentSources.Models;
using Shipbot.Controller.Core.Utilities;
using ApplicationSourceRepository = Shipbot.Controller.Core.Configuration.ApplicationSources.ApplicationSourceRepository;

namespace Shipbot.Controller.Core.Apps.Grains
{
    public interface IApplicationGrain : IGrainWithStringKey
    {
        Task RegisterEnvironment(ApplicationEnvironmentKey key);

        Task<IEnumerable<ApplicationEnvironmentKey>> GetEnvironments();
    }
    
    [StorageProvider(ProviderName = ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME)]
    public class ApplicationGrain : Grain<Application>, IApplicationGrain 
    {
        private readonly ILogger<ApplicationGrain> _log;

        public ApplicationGrain(ILogger<ApplicationGrain> log)
        {
            _log = log;
        }
        public override async Task OnActivateAsync()
        {
            State.Notifications = new NotificationSettings();

            var applicationIndex = GrainFactory.GetApplicationIndexGrain();
            await applicationIndex.EnsureRegistered(this.GetPrimaryKeyString());
            
            await base.OnActivateAsync();
        }

        

        public Task RegisterEnvironment(ApplicationEnvironmentKey key)
        {
            State.EnvironmentKeys.Add(key);
            return WriteStateAsync();
        }

        public Task<IEnumerable<ApplicationEnvironmentKey>> GetEnvironments() => Task.FromResult((IEnumerable<ApplicationEnvironmentKey>)State.EnvironmentKeys.ToArray());
    }
}