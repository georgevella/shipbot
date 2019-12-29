//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Linq;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using Shipbot.Controller.Core.Apps.Models;
//using Shipbot.Controller.Core.Configuration;
//using Shipbot.Controller.Core.Configuration.ApplicationSources;
//using Shipbot.Controller.Core.Configuration.Apps;
//using Shipbot.Controller.Core.ContainerRegistry.Watcher;
//using Shipbot.Controller.Core.DeploymentSources;
//using Shipbot.Controller.Core.DeploymentSources.Models;
//using Shipbot.Controller.Core.Git.Extensions;
//using Shipbot.Controller.Core.Models;
//using Shipbot.Controller.Core.Slack;
//using ApplicationSourceRepository = Shipbot.Controller.Core.DeploymentSources.Models.ApplicationSourceRepository;

namespace Shipbot.Controller.Core.Apps
{
//    public class ApplicationService : IApplicationService
//    {
//        private readonly ConcurrentDictionary<ImageTagKey, string> _currentTags = new ConcurrentDictionary<ImageTagKey, string>();
//        
//        private readonly ConcurrentDictionary<string, ApplicationContextData> _applications = new ConcurrentDictionary<string, ApplicationContextData>();
//
//        private readonly ILogger<ApplicationService> _log;
//        private readonly IApplicationSourceService _applicationSourceService;
//        private readonly IRegistryWatcher _registryWatcher;
//        private readonly IOptions<ShipbotConfiguration> _configuration;
//        private readonly ISlackClient _slackClient;
//
//        class ImageTagKey
//        {
//            protected bool Equals(ImageTagKey other)
//            {
//                return Equals(Application, other.Application) && Equals(Image, other.Image) && Equals(Environment, other.Environment);
//            }
//
//            public override bool Equals(object obj)
//            {
//                if (ReferenceEquals(null, obj)) return false;
//                if (ReferenceEquals(this, obj)) return true;
//                if (obj.GetType() != this.GetType()) return false;
//                return Equals((ImageTagKey) obj);
//            }
//
//            public override int GetHashCode()
//            {
//                unchecked
//                {
//                    var hashCode = (Application != null ? Application.GetHashCode() : 0);
//                    hashCode = (hashCode * 397) ^ (Image != null ? Image.GetHashCode() : 0);
//                    hashCode = (hashCode * 397) ^ (Environment != null ? Environment.GetHashCode() : 0);
//                    return hashCode;
//                }
//            }
//
//            public Application Application { get; }
//            public Image Image { get; }
//            public ApplicationEnvironment Environment { get; }
//
//            public ImageTagKey(Application application, Image image, ApplicationEnvironment environment)
//            {
//                Application = application;
//                Image = image;
//                Environment = environment;
//            }
//        }
//        class ApplicationContextData
//        {
//            public object Lock = new object();
//            
//            public Application Application { get; set; }
//            
//            public ApplicationSyncState State { get; set; }
//
//            public ApplicationContextData(Application application)
//            {
//                Application = application;
//                State = ApplicationSyncState.Unknown;
//            }
//        }
//
//        public ApplicationService(
//            ILogger<ApplicationService> log,
//            IApplicationSourceService applicationSourceService,
//            IRegistryWatcher registryWatcher,
//            IOptions<ShipbotConfiguration> configuration,
//            ISlackClient slackClient
//        )
//        {
//            _log = log;
//            _applicationSourceService = applicationSourceService;
//            _registryWatcher = registryWatcher;
//            _configuration = configuration;
//            _slackClient = slackClient;
//        }
//
//        public Application AddApplication(ApplicationDefinition applicationDefinition)
//        {
//            var conf = _configuration.Value;
//            
//            // TODO: check for unique name
//
//            var environments = applicationDefinition.Environments.Select(pair =>
//            {
//                var envDefinition = pair.Value;
//                var source = envDefinition.Source.Type switch
//                {
//                    ApplicationSourceType.Helm => (ApplicationSource) new HelmApplicationSource()
//                    {
//                        Repository = new ApplicationSourceRepository()
//                        {
//                            // TODO: handle config changes
//                            Credentials = conf.GitCredentials.FirstOrDefault(
//                                x =>
//                                    x.Name.Equals(envDefinition.Source.Repository.Credentials)
//                            ).ConvertToGitCredentials(),
//                            Ref = envDefinition.Source.Repository.Ref,
//                            Uri = new Uri(envDefinition.Source.Repository.Uri)
//                        },
//                        Path = envDefinition.Source.Path,
//                        ValuesFiles = envDefinition.Source.Helm.ValueFiles,
//                        SecretFiles = envDefinition.Source.Helm.Secrets,
//                    },
//                    _ => throw new InvalidOperationException()
//                };
//
//                var images = envDefinition.Images.Select(imageSettings => (Image) imageSettings).ToList();
//
//                return new ApplicationEnvironment(
//                    pair.Key,
//                    images, 
//                    source,
//                    envDefinition.AutoDeploy,
//                    envDefinition.PromoteTargets
//                    );
//            }).ToList();
//            
//
//            var application = new Application(
//                applicationDefinition.Name,
//                environments,
//                new NotificationSettings(applicationDefinition.SlackChannel)
//            );
//
//            _applications[application.Name] = new ApplicationContextData(application);
//
//            return application;
//        }
//
//        public IEnumerable<Application> GetApplications()
//        {
//            return _applications.Values.ToArray().Select( x=>x.Application ).ToArray();
//        }
//
//        public Application GetApplication(string id)
//        {
//            return _applications[id].Application;
//        }
//
////        public IReadOnlyDictionary<Image, string> GetCurrentImageTags(Application application, ApplicationEnvironment environment)
////        {
////            var result = new Dictionary<Image, string>();
////            foreach (var key in environment.Images.Select( image => new ImageTagKey(application, image, environment) ))
////            {
////                result.Add(key.Image, _currentTags[key]);
////            }
////            return result;
////        }
//
////        public void SetCurrentImageTag(
////            Application application, 
////            ApplicationEnvironment environment, 
////            Image image, 
////            string tag
////            )
////        {
////            var key = new ImageTagKey(application, image, environment);
////            _currentTags.AddOrUpdate(key,
////                (x, y) =>
////                {
////                    _log.LogInformation("Adding '{Repository}' to application {Application} with tag {Tag}",
////                        x.Image.Repository, y.application.Name, y.tag);
////                    return y.tag;
////                },  
////                (x, current, y) =>
////                {
////                    if (current == y.tag) 
////                        return current;
////                    
////                    _log.LogInformation(
////                        "Updating '{Repository}' with tag {Tag} for application {Application} with new tag {NewTag}",
////                        x.Image.Repository, current, y.application.Name, y.tag);
////                    return y.tag;
////
////                },
////                (application, tag)
////            );
////        }
//    }
}