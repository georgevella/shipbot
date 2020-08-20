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
        IReadOnlyDictionary<Image, string> GetCurrentImageTags(Application application);
        [Obsolete]
        void SetCurrentImageTag(Application application, Image image, string tag);
    }
}