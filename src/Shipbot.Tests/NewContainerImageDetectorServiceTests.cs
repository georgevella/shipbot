using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Octokit;
using Shipbot.Applications;
using Shipbot.Applications.Models;
using Shipbot.ContainerRegistry.Models;
using Shipbot.ContainerRegistry.Services;
using Shipbot.Deployments;
using Shipbot.Deployments.Internals;
using Shipbot.Models;
using Shipbot.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace Shipbot.Tests
{
    public class NewContainerImageDetectorServiceTests : BaseUnitTestClass
    {
        public NewContainerImageDetectorServiceTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
        
        [Fact]
        public void GetLatestTagMatchingPolicy_PositiveTest()
        {
            // setup
            var x = new DeploymentWorkflowService(
                GetLogger<DeploymentWorkflowService>(), 
                MockOf<IContainerImageMetadataService>(), 
                MockOf<IApplicationService>(), 
                Mock.Of<IApplicationImageInstanceService>(),
                MockOf<IDeploymentService>(), 
                MockOf<IDeploymentQueueService>(),
                Mock.Of<IGitHubClient>()
                );
            
            var imagePolicy = new GlobImageUpdatePolicy("develop-*");

            var expectedResult = new ContainerImage("testapp", "develop-256", DateTimeOffset.Now);
            var images = new[]
            {
                new ContainerImage("testapp", "master-123", DateTimeOffset.Now),
                new ContainerImage("testapp", "develop-123", DateTimeOffset.Now.AddDays(-1)),
                expectedResult,
            };

            // test
            var actualResult = x.GetLatestTagMatchingPolicy(images, imagePolicy);

            // verify
            actualResult.Should().BeEquivalentTo(expectedResult);
        }
        
        [Fact]
        public void GetLatestTagMatchingPolicy_EmptyImageList()
        {
            // setup
            var x = new DeploymentWorkflowService(
                GetLogger<DeploymentWorkflowService>(), 
                MockOf<IContainerImageMetadataService>(), 
                MockOf<IApplicationService>(), 
                Mock.Of<IApplicationImageInstanceService>(),
                MockOf<IDeploymentService>(), 
                MockOf<IDeploymentQueueService>(),
                Mock.Of<IGitHubClient>()

            );
            
            var imagePolicy = new GlobImageUpdatePolicy("develop-*");

            // test
            Action func = () => x.GetLatestTagMatchingPolicy(new ContainerImage[] { }, imagePolicy);

            // verify
            func.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ComparerTest_Glob_MoreRecentImageVsCurrentImage()
        {
            // setup
            var x = new DeploymentWorkflowService(
                GetLogger<DeploymentWorkflowService>(), 
                MockOf<IContainerImageMetadataService>(), 
                MockOf<IApplicationService>(), 
                Mock.Of<IApplicationImageInstanceService>(),
                MockOf<IDeploymentService>(), 
                MockOf<IDeploymentQueueService>(),
            Mock.Of<IGitHubClient>()

            );
            
            var currentImage = new ContainerImage("testapp", "develop-123", DateTimeOffset.Now.AddDays(-1));
            var expectedResult = new ContainerImage("testapp", "develop-256", DateTimeOffset.Now);

            var imagePolicy = new GlobImageUpdatePolicy("develop-*");

            var comparer = x.GetContainerImageComparer(imagePolicy);

            // test
            var compare = comparer.Compare(currentImage, expectedResult);

            // verify
            compare.Should().BeLessThan(0);
        }
        
        [Fact]
        public void ComparerTest_Glob_OlderImageVsCurrentImage()
        {
            // setup
            var x = new DeploymentWorkflowService(
                GetLogger<DeploymentWorkflowService>(), 
                MockOf<IContainerImageMetadataService>(), 
                MockOf<IApplicationService>(), 
                Mock.Of<IApplicationImageInstanceService>(),
                MockOf<IDeploymentService>(), 
                MockOf<IDeploymentQueueService>(),
                Mock.Of<IGitHubClient>()
            );
            
            var currentImage = new ContainerImage("testapp", "develop-123", DateTimeOffset.Now.AddDays(-1));
            var expectedResult = new ContainerImage("testapp", "develop-256", DateTimeOffset.Now.AddDays(-2));

            var imagePolicy = new GlobImageUpdatePolicy("develop-*");

            var comparer = x.GetContainerImageComparer(imagePolicy);

            // test
            var compare = comparer.Compare(currentImage, expectedResult);

            // verify
            compare.Should().BeGreaterThan(0);
        }
        
        [Fact]
        public void ComparerTest_Glob_Equality()
        {
            // setup
            var x = new DeploymentWorkflowService(
                GetLogger<DeploymentWorkflowService>(), 
                MockOf<IContainerImageMetadataService>(), 
                MockOf<IApplicationService>(), 
                Mock.Of<IApplicationImageInstanceService>(),
                MockOf<IDeploymentService>(), 
                MockOf<IDeploymentQueueService>(),
                Mock.Of<IGitHubClient>()
            );

            var creationDateTime = DateTimeOffset.Now.AddDays(-1);
            var currentImage = new ContainerImage("testapp", "develop-123", creationDateTime);
            var expectedResult = new ContainerImage("testapp", "develop-123", creationDateTime);

            var imagePolicy = new GlobImageUpdatePolicy("develop-*");

            var comparer = x.GetContainerImageComparer(imagePolicy);

            // test
            var compare = comparer.Compare(currentImage, expectedResult);

            // verify
            compare.Should().Be(0);
        }
    }
}