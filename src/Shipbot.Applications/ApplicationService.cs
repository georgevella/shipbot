using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shipbot.Contracts;
using Shipbot.Controller.Core.ApplicationSources;
using Shipbot.Controller.Core.Configuration;
using Shipbot.Controller.Core.Configuration.ApplicationSources;
using Shipbot.Controller.Core.Configuration.Apps;
using Shipbot.Controller.Core.Registry.Watcher;
using Shipbot.Controller.Core.Slack;
using Shipbot.Models;
using ApplicationSourceRepository = Shipbot.Models.ApplicationSourceRepository;

namespace Shipbot.Controller.Core.Apps
{
    public class ApplicationService : IApplicationService
    {
        private readonly ILogger<ApplicationService> _log;
        private readonly IApplicationSourceService _applicationSourceService;
        private readonly IRegistryWatcher _registryWatcher;
        private readonly IOptions<ShipbotConfiguration> _configuration;
        private readonly ISlackClient _slackClient;
        private readonly IApplicationStore _applicationStore;

        public ApplicationService(
            ILogger<ApplicationService> log,
            IApplicationSourceService applicationSourceService,
            IRegistryWatcher registryWatcher,
            IOptions<ShipbotConfiguration> configuration,
            ISlackClient slackClient,
            IApplicationStore applicationStore
        )
        {
            _log = log;
            _applicationSourceService = applicationSourceService;
            _registryWatcher = registryWatcher;
            _configuration = configuration;
            _slackClient = slackClient;
            _applicationStore = applicationStore;
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

            _applicationStore.AddApplication(application);

            return application;
        }

        public IEnumerable<Application> GetApplications() => _applicationStore.GetAllApplications();

        public Application GetApplication(string id)
        {
            if (_applicationStore.ApplicationExists(id))
            {
                return _applicationStore.GetApplication(id);
            }
            
            throw new KeyNotFoundException(id);
        }

        public async Task StartTrackingApplication(Application application)
        {
            await _applicationSourceService.AddApplicationSource(application);
            await _registryWatcher.StartWatchingImageRepository(application);
        }

        [Obsolete]
        public IReadOnlyDictionary<Image, string> GetCurrentImageTags(Application application)
        {
            return _applicationStore.GetCurrentImageTags(application);
        }

        [Obsolete]
        public void SetCurrentImageTag(Application application, Image image, string tag)
        {
            _applicationStore.SetCurrentImageTag(application, image, tag);
        }
    }
}