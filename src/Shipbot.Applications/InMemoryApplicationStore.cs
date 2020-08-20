using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Shipbot.Models;

namespace Shipbot.Applications
{
    public class InMemoryApplicationStore : IApplicationStore
    {
        private readonly ILogger<InMemoryApplicationStore> _log;
        private readonly ConcurrentDictionary<string, ApplicationContextData> _applications = new ConcurrentDictionary<string, ApplicationContextData>();
        
        class ApplicationContextData
        {
            public object Lock = new object();
            
            public Application Application { get; set; }
            
            public ApplicationSyncState State { get; set; }
            
            public ConcurrentDictionary<Image, string> CurrentTags { get; } = new ConcurrentDictionary<Image, string>();
            
            public ApplicationContextData(Application application)
            {
                Application = application;
                State = ApplicationSyncState.Unknown;
            }
        }

        public InMemoryApplicationStore(ILogger<InMemoryApplicationStore> log)
        {
            _log = log;
        }

        public void AddApplication(Application application)
        {
            _applications[application.Name] = new ApplicationContextData(application);
        }

        public IEnumerable<Application> GetAllApplications() => _applications.Values.ToArray().Select( x=>x.Application ).ToArray();

        public bool Contains(string name) => _applications.ContainsKey(name);

        public Application GetApplication(string name) => _applications[name].Application;

        // TODO: move to dedicated service (application source service, or a new impl)
        [Obsolete]
        public IReadOnlyDictionary<Image, string> GetCurrentImageTags(Application application)
        {
            return _applications[application.Name].CurrentTags;
        }

        // TODO: move to dedicated service (application source service, or a new impl)
        [Obsolete]
        public void SetCurrentImageTag(Application application, Image image, string tag)
        {
            var ctx = _applications[application.Name];
            ctx.CurrentTags.AddOrUpdate(image,
                (x, y) =>
                {
                    _log.LogInformation("Adding '{Repository}' to application {Application} with tag {Tag}",
                        x.Repository, y.application.Name, y.tag);
                    return y.tag;
                },  
                (x, current, y) =>
                {
                    if (current == y.tag) 
                        return current;
                    
                    _log.LogInformation(
                        "Updating '{Repository}' with tag {Tag} for application {Application} with new tag {NewTag}",
                        x.Repository, current, y.application.Name, y.tag);
                    return y.tag;

                },
                (application, tag)
            );
        }
    }
}