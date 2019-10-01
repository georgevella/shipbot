using System.Collections.Generic;
using System.Threading.Tasks;
using Shipbot.Controller.Core.Configuration.Apps;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Apps
{
    public interface IApplicationService
    {
        Application AddApplication(ApplicationDefinition applicationDefinition);
        IEnumerable<Application> GetApplications();
        Application GetApplication(string id);

        void SetCurrentImageTag(Application application, ApplicationEnvironment environment, Image image, string tag);
        IReadOnlyDictionary<Image, string> GetCurrentImageTags(Application application, ApplicationEnvironment environment);
    }
}