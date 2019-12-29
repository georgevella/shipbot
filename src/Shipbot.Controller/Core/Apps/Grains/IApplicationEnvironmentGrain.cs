using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Orleans;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Configuration.Apps;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Apps.Grains
{
    public interface IApplicationEnvironmentGrain : IGrainWithStringKey
    {
        Task<IEnumerable<Image>> GetImages();
        Task EnableAutoDeploy();
        Task DisableAutoDeploy();
        Task SetImageTag(Image image, string newImageTag);
        Task<IReadOnlyDictionary<Image, string>> GetCurrentImageTags();
        Task Configure(ApplicationEnvironmentSettings applicationEnvironmentSettings);
        Task<IEnumerable<string>> GetDeploymentPromotionSettings();
        Task<ImageUpdatePolicy> GetImageUpdatePolicy(Image image);
        Task<bool> IsAutoDeployEnabled();
        Task CheckForMissedImageTags();
        Task StartListeningToImageTagUpdates();
        Task StopListeningToImageTagUpdates();
    }
}