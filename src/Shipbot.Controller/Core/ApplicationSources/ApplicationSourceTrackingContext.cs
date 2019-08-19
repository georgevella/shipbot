using System.Collections.Concurrent;
using System.IO;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.ApplicationSources
{
    public class ApplicationSourceTrackingContext
    {
        public Application Application { get; }
        
        //public ConcurrentDictionary<Image, string> CurrentTags { get; }

        public string GitRepositoryPath { get; }

        public string GitSyncJobId { get; } 

        public ApplicationSourceTrackingContext(Application application)
        {
            GitRepositoryPath = Path.Combine(Path.GetTempPath(), $"{application.Name}__{application.Source.Repository.Ref}");
            Application = application;
            GitSyncJobId = $"{Application.Name}-gitsync-job";
            
            //CurrentTags = new ConcurrentDictionary<Image, string>();
        }
    }
}