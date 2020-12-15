using System;
using System.Collections.Generic;
using Shipbot.Models;

namespace Shipbot.Applications
{
    public interface IApplicationStore
    {
        void AddApplication(Application application);
        IEnumerable<Application> GetAllApplications();
        bool Contains(string name);
        Application GetApplication(string name);
        [Obsolete]
        IReadOnlyDictionary<ApplicationImage, string> GetCurrentImageTags(Application application);
        [Obsolete]
        void SetCurrentImageTag(Application application, ApplicationImage image, string tag);

        void ReplaceApplication(Application application);
    }
}