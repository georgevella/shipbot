using System;
using System.Collections.Generic;
using Shipbot.Controller.Core.Configuration.Apps;
using Shipbot.Models;

namespace Shipbot.Applications
{
    public interface IApplicationService
    {
        Application AddApplication(ApplicationDefinition applicationDefinition);
        IEnumerable<Application> GetApplications();
        Application GetApplication(string id);

        [Obsolete]
        void SetCurrentImageTag(Application application, ApplicationImage image, string tag);
        [Obsolete]
        IReadOnlyDictionary<ApplicationImage, string> GetCurrentImageTags(Application application);
    }
}