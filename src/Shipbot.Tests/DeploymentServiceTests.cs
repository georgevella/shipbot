using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Shipbot.Controller.Core.Apps;
using Shipbot.Controller.Core.Configuration.ApplicationSources;
using Shipbot.Controller.Core.Configuration.Apps;
using Shipbot.Controller.Core.Deployments;
using Shipbot.Models;
using Xunit;

namespace Shipbot.Tests
{

    public class DeploymentServiceTests
    {
        private readonly IApplicationService _applicationService;
        private readonly IDeploymentService _deploymentService;

        public DeploymentServiceTests(
            IApplicationService applicationService,
            IDeploymentService deploymentService
            )
        {
            _applicationService = applicationService;
            _deploymentService = deploymentService;
        }
        
        [Fact]
        public void X()
        {
            _applicationService.AddApplication(new ApplicationDefinition()
            {
                Name = "TestApplication",
                AutoDeploy = true,
                Images = new List<ImageSettings>()
                {
                    new ImageSettings()
                    {
                        Pattern = "master-*",
                        Policy = UpdatePolicy.Glob,
                        Repository = "repository/image",
                        TagProperty = new TagPropertySettings()
                        {
                            Path = "image.tag",
                            ValueFormat = TagPropertyValueFormat.TagOnly
                        }
                    }
                },
                Source = new ApplicationSourceSettings()
                {
                    Helm = new HelmApplicationSourceSettings()
                    {
                        
                    }
                } 
            });

            var application = _applicationService.GetApplication("TestApplication");
            var image = application.Images.First();

            _deploymentService.AddDeploymentUpdate(application, image, "123");
        }
    }
}