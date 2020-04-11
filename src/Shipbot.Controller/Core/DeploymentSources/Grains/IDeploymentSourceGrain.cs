using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;
using Orleans;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Configuration.ApplicationSources;
using Shipbot.Controller.Core.Deployments.GrainKeys;
using Shipbot.Controller.Core.Deployments.Models;
using Shipbot.Controller.Core.DeploymentSources.Models;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.DeploymentSources.Grains
{
    public interface IDeploymentSourceGrain : IGrainWithStringKey
    {
        Task Activate();
        Task Checkout();

        Task Configure(
            ApplicationSourceSettings applicationSourceSettings,
            ApplicationEnvironmentKey applicationEnvironmentKey
        );

        Task Refresh();
        Task<IReadOnlyDictionary<string, string>> GetImageTags();
            
        Task<DeploymentSourceChangeResult> ApplyDeploymentAction(DeploymentSourceChange deploymentSourceChange);
    }
}