using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shipbot.Controller.Core.ApplicationSources;
using Shipbot.Controller.Core.Configuration;
using Shipbot.Controller.Core.Configuration.ApplicationSources;
using Shipbot.Controller.Core.Configuration.Apps;
using Shipbot.Controller.Core.Models;
using Shipbot.Controller.Core.Registry.Watcher;
using Shipbot.Controller.Core.Slack;
using ApplicationSourceRepository = Shipbot.Controller.Core.Models.ApplicationSourceRepository;

namespace Shipbot.Controller.Core.Apps
{
    public class ApplicationService : IApplicationService
    {
        private readonly ConcurrentDictionary<string, ApplicationContextData> _applications = new ConcurrentDictionary<string, ApplicationContextData>();

        private readonly ILogger<ApplicationService> _log;
        private readonly IApplicationSourceService _applicationSourceService;
        private readonly IRegistryWatcher _registryWatcher;
        private readonly IOptions<ShipbotConfiguration> _configuration;
        private readonly ISlackClient _slackClient;

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

        public ApplicationService(
            ILogger<ApplicationService> log,
            IApplicationSourceService applicationSourceService,
            IRegistryWatcher registryWatcher,
            IOptions<ShipbotConfiguration> configuration,
            ISlackClient slackClient
        )
        {
            _log = log;
            _applicationSourceService = applicationSourceService;
            _registryWatcher = registryWatcher;
            _configuration = configuration;
            _slackClient = slackClient;
        }

        public Application AddApplication(ApplicationDefinition applicationDefinition)
        {
            var conf = _configuration.Value;
            
            // TODO: check for unique name

            var applicationSource = applicationDefinition.Source.Type switch {
                ApplicationSourceType.Helm => (ApplicationSource) new HelmApplicationSource()
                {
                    Repository = new ApplicationSourceRepository()
                    {
                        // TODO: handle config changes
                        Credentials = conf.GitCredentials.FirstOrDefault(
                            x =>
                                x.Name.Equals(applicationDefinition.Source.Repository.Credentials)
                        ).ConvertToGitCredentials(),
                        Ref = applicationDefinition.Source.Repository.Ref,
                        Uri = new Uri(applicationDefinition.Source.Repository.Uri)
                    },
                    Path = applicationDefinition.Source.Path,
                    ValuesFiles = applicationDefinition.Source.Helm.ValueFiles,
                    Secrets = applicationDefinition.Source.Helm.Secrets,
                },
                _ => throw new InvalidOperationException() 
            };

            var application = new Application(
                applicationDefinition.Name,
                applicationDefinition.Images.Select(imageSettings => (Image) imageSettings).ToImmutableList(),
                applicationSource,
                applicationDefinition.AutoDeploy,
                new NotificationSettings(applicationDefinition.SlackChannel)
            );

            _applications[application.Name] = new ApplicationContextData(application);

            return application;
        }

        public IEnumerable<Application> GetApplications()
        {
            return _applications.Values.ToArray().Select( x=>x.Application ).ToArray();
        }

        public Application GetApplication(string id)
        {
            return _applications[id].Application;
        }

        public async Task StartTrackingApplication(Application application)
        {
            await _applicationSourceService.AddApplicationSource(application);
            await _registryWatcher.StartWatchingImageRepository(application);
        }

        public IReadOnlyDictionary<Image, string> GetCurrentImageTags(Application application)
        {
            return _applications[application.Name].CurrentTags;
        }

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