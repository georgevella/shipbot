using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shipbot.Controller.Core.Configuration;
using Shipbot.Controller.Core.Configuration.ApplicationSources;
using Shipbot.Controller.Core.Configuration.Apps;
using Shipbot.Models;
// using ApplicationSourceRepository = Shipbot.Models.ApplicationSourceRepository;

namespace Shipbot.Applications
{
    public class ApplicationService : IApplicationService
    {
        private readonly ILogger<ApplicationService> _log;
        private readonly IOptions<ShipbotConfiguration> _configuration;
        private readonly IApplicationStore _applicationStore;

        public ApplicationService(
            ILogger<ApplicationService> log,
            IOptions<ShipbotConfiguration> configuration,
            IApplicationStore applicationStore
        )
        {
            _log = log;
            _configuration = configuration;
            _applicationStore = applicationStore;
        }

        public Application AddApplication(string name, ApplicationDefinition applicationDefinition)
        {
            var conf = _configuration.Value;

            if (_applicationStore.Contains(name))
            {
                throw new Exception($"An application with the name '{name}' already exists.");
            }

            var application = new Application(
                name,
                applicationDefinition.Images.Select(imageSettings => (ApplicationImage) imageSettings).ToImmutableList(),
                // applicationSource,
                applicationDefinition.AutoDeploy,
                new NotificationSettings(applicationDefinition.SlackChannel)
            );

            _applicationStore.AddApplication(application);

            return application;
        }

        public IEnumerable<Application> GetApplications() => _applicationStore.GetAllApplications();

        public Application GetApplication(string id)
        {
            if (_applicationStore.Contains(id))
            {
                return _applicationStore.GetApplication(id);
            }
            
            throw new Exception($"Application '{id}' not found in store");
        }

        [Obsolete]
        public IReadOnlyDictionary<ApplicationImage, string> GetCurrentImageTags(Application application)
        {
            return _applicationStore.GetCurrentImageTags(application);
        }

        [Obsolete]
        public void SetCurrentImageTag(Application application, ApplicationImage image, string tag)
        {
            _applicationStore.SetCurrentImageTag(application, image, tag);
        }
    }
}