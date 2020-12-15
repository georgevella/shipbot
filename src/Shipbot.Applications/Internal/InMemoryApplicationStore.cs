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
        private readonly object _lock = new object();
        private readonly ILogger<InMemoryApplicationStore> _log;
        private readonly Dictionary<string, ApplicationContextData> _applications = new Dictionary<string, ApplicationContextData>();

        private readonly Dictionary<string, List<ApplicationContextData>> _containerRepositoryMap =
            new Dictionary<string, List<ApplicationContextData>>();
        
        class ApplicationContextData
        {
            public object Lock = new object();
            
            public Application Application { get; set; }
            
            public ApplicationSyncState State { get; set; }
            
            public ConcurrentDictionary<ApplicationImage, string> CurrentTags { get; } = new ConcurrentDictionary<ApplicationImage, string>();
            
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
            lock (_lock)
            {
                if (_applications.ContainsKey(application.Name))
                    throw new InvalidOperationException("Application already defined.");
                var applicationContextData = new ApplicationContextData(application);
                _applications[application.Name] = applicationContextData;

                foreach (var i in application.Images)
                {
                    if (!_containerRepositoryMap.ContainsKey(i.Repository))
                        _containerRepositoryMap.Add(i.Repository, new List<ApplicationContextData>());
                    
                    _containerRepositoryMap[i.Repository].Add(applicationContextData);
                }
            }
        }

        public IEnumerable<Application> GetAllApplications()
        {
            lock (_lock)
                return _applications.Values.ToArray().Select(x => x.Application).ToArray();
        }

        public bool Contains(string name)
        {
            lock (_lock)
                return _applications.ContainsKey(name);
        }

        public Application GetApplication(string name)
        {
            lock (_lock)
                return _applications[name].Application;
        }

        // TODO: move to dedicated service (application source service, or a new impl)
        [Obsolete]
        public IReadOnlyDictionary<ApplicationImage, string> GetCurrentImageTags(Application application)
        {
            lock (_lock)
            {
                return _applications[application.Name].CurrentTags;
            }
        }

        // TODO: move to dedicated service (application source service, or a new impl)
        [Obsolete]
        public void SetCurrentImageTag(Application application, ApplicationImage image, string tag)
        {
            lock (_lock)
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

        public void ReplaceApplication(Application application)
        {
            lock (_lock)
            {
                if (!_applications.ContainsKey(application.Name)) 
                    return;
                
                var ctx = _applications[application.Name];
                ctx.Application = application;
            }
        }
    }
}