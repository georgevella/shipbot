using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shipbot.Applications.Models;
using Shipbot.Controller.Core.Configuration.Apps;
using Shipbot.Models;

namespace Shipbot.Applications
{
    public interface IApplicationService
    {
        Application AddApplication(string name, ApplicationDefinition applicationDefinition);
        IEnumerable<Application> GetApplications();
        Application GetApplication(string id);

        // [Obsolete]
        // void SetCurrentImageTag(Application application, ApplicationImage image, string tag);
        // [Obsolete]
        // IReadOnlyDictionary<ApplicationImage, string> GetCurrentImageTags(Application application);

        Task ChangeApplicationDeploymentSettings(
            string application, 
            ApplicationImage image,
            DeploymentSettings deploymentSettings);
    }
}