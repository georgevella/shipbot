using System.Collections.Concurrent;
using System.IO;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.ApplicationSources
{
    public class ApplicationSourceTrackingContext
    {
        public Application Application { get; }
        
        //public ConcurrentDictionary<Image, string> CurrentTags { get; }

        public DirectoryInfo GitRepositoryPath { get; }

        public string GitSyncJobId { get; } 

        public ApplicationSourceTrackingContext(Application application, ApplicationEnvironment applicationEnvironment)
        {
            GitRepositoryPath = new DirectoryInfo(
                Path.Combine(Path.GetTempPath(), $"{application.Name}_{applicationEnvironment.Name}__{applicationEnvironment.Source.Repository.Ref}")
                ); 
            Application = application;
            Environment = applicationEnvironment;
            GitSyncJobId = $"{Application.Name}-{applicationEnvironment.Name}-gitsync-job";
            
            //CurrentTags = new ConcurrentDictionary<Image, string>();
        }

        public ApplicationEnvironment Environment { get; }
    }
}