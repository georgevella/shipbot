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
        void SetCurrentImageTag(Application application, Image image, string tag);
        [Obsolete]
        IReadOnlyDictionary<Image, string> GetCurrentImageTags(Application application);
    }
}