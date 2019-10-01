using System.Threading.Tasks;
using Shipbot.Controller.Core.ApplicationSources.Sync;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.ApplicationSources
{
    public interface IApplicationSourceSyncService
    {
        Task ApplyDeploymentUpdates(
            HelmApplicationSourceDetails helmApplicationSourceDetails,
            DeploymentUpdate deploymentUpdate);

        Task<HelmApplicationSourceDetails> BuildApplicationSourceDetails(
            ApplicationSourceTrackingContext context,
            HelmApplicationSource helmApplicationSource
        );
    }
}