using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Shipbot.Controller.Core.Apps;
using Shipbot.Controller.Core.Configuration;
using Shipbot.Controller.Core.Deployments;
using Shipbot.Controller.Core.Models;
using Shipbot.Controller.Core.Slack;
using Shipbot.Tests.Generators;
using Xunit;
using Xunit.Abstractions;

namespace Shipbot.Tests
{
    public class DeploymentServiceTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private const string SlackUserToken = "";

        public DeploymentServiceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        

        private Application BuildApplication(string name = "MockedApplication", IEnumerable<ApplicationEnvironment> environments = null)
        {
            return new Application(name, environments, new NotificationSettings("slack-bots-and-more"));
        }
        
        private Mock<IApplicationService> BuildApplicationServiceMock(Application application)
        {
            return BuildApplicationServiceMock(new[] {application});
        }

        private Mock<IApplicationService> BuildApplicationServiceMock(IEnumerable<Application> applications)
        {
            var mock = new Mock<IApplicationService>();
            
            mock.Setup(svc => svc.GetApplications()).Returns(applications);
            mock.Setup(svc => svc.GetApplication(It.Is<string>(x => x == "MockedApplication")))
                .Returns(applications.First());
            return mock;
        }

        public Mock<ISlackClient> BuildSlackClientMock()
        {
            return new Mock<ISlackClient>();
        }
        
        [Fact]
        public async Task CreateNewDeploymentWithAutoDeployEnvironment_ShouldHaveDeploymentUpdatePending()
        {
            // Arrange
            var environment = EnvironmentGenerator.BuildEnvironment();
            var application = BuildApplication(environments: new[] {environment});
            var buildApplicationServiceMock = BuildApplicationServiceMock(application);
            
            var services = new ServiceCollection()
                .AddLogging((builder) => builder.AddXUnit(_testOutputHelper))
                //.Configure<SlackConfiguration>(configuration => configuration.Token = SlackUserToken)
                .AddSingleton<ISlackClient>(BuildSlackClientMock().Object)
                .AddSingleton<ISlackClient, SlackClient>()
                .AddSingleton<IApplicationService>(buildApplicationServiceMock.Object)
                .AddSingleton<IDeploymentService, DeploymentService>();

            var sp = services.BuildServiceProvider();
            var slack = sp.GetService<ISlackClient>();

            await slack.Connect();
            
            var deploymentService = sp.GetRequiredService<IDeploymentService>();

            buildApplicationServiceMock.Setup(svc => svc.GetCurrentImageTags(application, environment)).Returns(
                new Dictionary<Image, string>()
                {
                    {environment.Images.First(), "develop-1"}
                }
            );

            // Act
            await deploymentService.AddDeploymentUpdate(application, environment, environment.Images.First(), "develop-123");

            // Assert
            var deploymentUpdate = await deploymentService.GetNextPendingDeploymentUpdate(application, environment);
            deploymentUpdate.TargetTag.Should().Be("develop-123");
        }
        
        [Fact]
        public async Task CreateNewDeploymentWithAutoDeployEnvironment_ShouldShowPromoteButtonsOnSlackMessage()
        {
            // Arrange
            var dev = EnvironmentGenerator.BuildEnvironment(promotionList: new List<string>() { "test" });
            var test = EnvironmentGenerator.BuildEnvironment(name: "test");
            var application = BuildApplication(environments: new[] {dev, test});
            var buildApplicationServiceMock = BuildApplicationServiceMock(application);
            
            var services = new ServiceCollection()
                .AddLogging((builder) => builder.AddXUnit(_testOutputHelper))
                //.Configure<SlackConfiguration>(configuration => configuration.Token = SlackUserToken)
                .AddSingleton<ISlackClient>(BuildSlackClientMock().Object)
                .AddSingleton<ISlackClient, SlackClient>()
                .AddSingleton<IApplicationService>(buildApplicationServiceMock.Object)
                .AddSingleton<IDeploymentService, DeploymentService>();

            var sp = services.BuildServiceProvider();
            var slack = sp.GetService<ISlackClient>();

            await slack.Connect();
            
            var deploymentService = sp.GetRequiredService<IDeploymentService>();

            buildApplicationServiceMock.Setup(svc => svc.GetCurrentImageTags(application, dev)).Returns(
                new Dictionary<Image, string>()
                {
                    {dev.Images.First(), "develop-1"}
                }
            );

            buildApplicationServiceMock.Setup(svc => svc.GetCurrentImageTags(application, test)).Returns(
                new Dictionary<Image, string>()
                {
                    
                }
            );

            
            // Act
            await deploymentService.AddDeploymentUpdate(application, dev, dev.Images.First(), "develop-123");

            // Assert
            var deploymentUpdate = await deploymentService.GetNextPendingDeploymentUpdate(application, dev);
            deploymentUpdate.TargetTag.Should().Be("develop-123");

            await deploymentService.FinishDeploymentUpdate(deploymentUpdate, DeploymentUpdateStatus.Complete);
        }
        
        [Fact]
        public async Task CreateNewDeploymentWithAutoDeployEnvironment_ShouldShowPromoteButtonsOnSlackMessage_WithPromotion()
        {
            // Arrange
            var dev = EnvironmentGenerator.BuildEnvironment(promotionList: new List<string>() { "test" });
            var test = EnvironmentGenerator.BuildEnvironment(name: "test");
            var application = BuildApplication(environments: new[] {dev, test});
            var buildApplicationServiceMock = BuildApplicationServiceMock(application);
            
            var services = new ServiceCollection()
                .AddLogging((builder) => builder.AddXUnit(_testOutputHelper))
                .Configure<SlackConfiguration>(configuration => configuration.Token = SlackUserToken)
                //.AddSingleton<ISlackClient>(BuildSlackClientMock().Object)
                .AddSingleton<ISlackClient, SlackClient>()
                .AddSingleton<IApplicationService>(buildApplicationServiceMock.Object)
                .AddSingleton<IDeploymentService, DeploymentService>();

            var sp = services.BuildServiceProvider();
            var slack = sp.GetService<ISlackClient>();

            await slack.Connect();
            
            var deploymentService = sp.GetRequiredService<IDeploymentService>();

            buildApplicationServiceMock.Setup(svc => svc.GetCurrentImageTags(application, dev)).Returns(
                new Dictionary<Image, string>()
                {
                    {dev.Images.First(), "develop-1"}
                }
            );
            buildApplicationServiceMock.Setup(svc => svc.GetCurrentImageTags(application, test)).Returns(
                new Dictionary<Image, string>()
                {
                    
                }
            );
            
            // Act
            await deploymentService.AddDeploymentUpdate(application, dev, dev.Images.First(), "develop-123");
            
            var deploymentUpdate = await deploymentService.GetNextPendingDeploymentUpdate(application, dev);
            deploymentUpdate.TargetTag.Should().Be("develop-123");

            await deploymentService.FinishDeploymentUpdate(deploymentUpdate, DeploymentUpdateStatus.Complete);
            
            await Task.Delay(1000);
            
            await deploymentService.PromoteDeployment(deploymentUpdate);
            
            var promotedDeploymentUpdate = await deploymentService.GetNextPendingDeploymentUpdate(application, test);
            promotedDeploymentUpdate.TargetTag.Should().Be("develop-123");
            
            await Task.Delay(1000);
            
            await deploymentService.FinishDeploymentUpdate(promotedDeploymentUpdate, DeploymentUpdateStatus.Complete);
        }
        
        
        // TODO:
        // channel for notifications does not exist
    }
}