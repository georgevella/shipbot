using System.Collections.Generic;
using System.Threading.Tasks;
using Shipbot.Controller.Core.Configuration.Apps;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Apps
{
    public interface IApplicationService
    {
        Task StartTrackingApplication(Application application);
        Application AddApplication(ApplicationDefinition applicationDefinition);
        IEnumerable<Application> GetApplications();
        Application GetApplication(string id);

        void SetCurrentImageTag(Application application, Image image, string tag);
        IReadOnlyDictionary<Image, string> GetCurrentImageTags(Application application);
    }
}