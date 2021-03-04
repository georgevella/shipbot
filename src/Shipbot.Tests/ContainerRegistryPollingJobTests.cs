using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Shipbot.Applications;
using Shipbot.Applications.Models;
using Shipbot.ContainerRegistry;
using Shipbot.ContainerRegistry.Internals;
using Shipbot.ContainerRegistry.Models;
using Shipbot.ContainerRegistry.Services;
using Shipbot.ContainerRegistry.Watcher;
using Shipbot.Deployments;
using Shipbot.Deployments.Models;
using Shipbot.Models;
using Shipbot.Tests.Utils;
using Slack.NetStandard.ApiCommon;
using Xunit;
using Xunit.Abstractions;

namespace Shipbot.Tests
{
    public class ContainerRegistryPollingJobTests : BaseUnitTestClass
    {
        public ContainerRegistryPollingJobTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory]
        [InlineData("develop-*", "develop-123", "develop-256", 1)]
        [InlineData("develop-*", "develop-123", "master-256", 0)]
        [InlineData("develop-*", "develop-123", null, 0)]
        [InlineData("develop-*", null, null, 0)]
        [InlineData("develop-*", null, "develop-256", 0)]
        [InlineData("develop-*", null, "master-256", 0)]
        public async Task DetectNewTags(string imagePattern, string? currentImageTag, string? newerImageTag, int createdDeployments)
        {
            // setup
            UseStrictMocks();
            
            const string imageRepository = "test-image";
            const string applicationId = "test-app";
            var deploymentId = Guid.NewGuid();
            
            var containerImages = new List<ContainerImage>();

            ContainerImage? currentContainerImage = null;
            if (currentImageTag != null)
            {
                currentContainerImage = new ContainerImage(imageRepository, currentImageTag,
                    DateTimeOffset.Now.AddDays(-1)
                );
                containerImages.Add(currentContainerImage);
            }

            if (newerImageTag != null)
            {
                containerImages.Add(
                    new ContainerImage(imageRepository, newerImageTag,
                        DateTimeOffset.Now
                    )
                );
            }
            
            var registryClient = MockOf<IRegistryClient>(
                mock =>
                {
                    if (currentContainerImage != null)
                    {
                        mock
                            .Setup(x => x.GetImage(
                                    It.Is<string>(s => s.Equals(imageRepository)),
                                    It.Is<string>(s => s.Equals(currentImageTag))
                                )
                            )
                            .Returns(Task.FromResult(currentContainerImage));    
                    }
                    
                    mock.Setup(
                            x => x.GetRepositoryTags(It.Is<string>(s => s.Equals(imageRepository)))
                        )
                        .Returns(Task.FromResult(containerImages.AsEnumerable()));
                });
            
            var registryClientPool = MockOf<IRegistryClientPool>(
                mock => mock
                    .Setup(x=>x.GetRegistryClientForRepository(It.Is<string>(s => s == imageRepository)))
                    .Returns( Task.FromResult(registryClient))
            );

            var applicationImage = new ApplicationImage(imageRepository,
                new TagProperty("image.tag", TagPropertyValueFormat.TagOnly),
                new GlobImageUpdatePolicy(imagePattern),
                new DeploymentSettings(true, true),
                ApplicationImageSourceCode.Empty,
                ApplicationImageIngress.Empty
            );
            
            var application = new Application(
                applicationId,
                new[]
                {
                    applicationImage
                },
                new NotificationSettings("#abc")
            );
            var applicationService = MockOf<IApplicationService>(
                mock =>
                {
                    mock.Setup(x => x.GetApplication(It.Is<string>(s => s == applicationId)))
                        .Returns(application);

                    var currentImageTags = currentImageTag != null
                        ? new Dictionary<ApplicationImage, string>()
                        {
                            {applicationImage, currentImageTag}
                        }
                        : new Dictionary<ApplicationImage, string>();

                    // mock.Setup(
                    //         x => x.GetCurrentImageTags(
                    //             It.Is<Application>(p => p.Equals(application))
                    //         )
                    //     )
                    //     .Returns(currentImageTags);
                });

            var deploymentService = MockOf<IDeploymentService>(
                mock =>
                {
                    mock.Setup(
                            x => x.AddDeployment(
                                It.Is<Application>(p => p.Equals(application)),
                                It.Is<ApplicationImage>(p => p.Equals(applicationImage)),
                                It.Is<string>(s => s.Equals(newerImageTag)),
                                It.Is<DeploymentType>( t => t.Equals(DeploymentType.ImageUpdate)),
                                It.Is<string>( s => s == string.Empty),
                                It.IsAny<IReadOnlyDictionary<string,string>>()
                            )
                        )
                        .Callback<Application,ApplicationImage,string>((application,image,newTag) => {})
                        .ReturnsAsync(
                            new Deployment(
                                deploymentId, 
                                applicationId, 
                                imageRepository, 
                                "image.tag", currentImageTag, newerImageTag, DeploymentStatus.Pending,
                                DeploymentType.ImageUpdate,
                                string.Empty,
                                DateTime.Now.AddMinutes(-1),
                                DateTime.Now,
                                string.Empty)
                        );
                },
                mock =>
                {
                    mock.Verify(
                        x => x.AddDeployment(
                            It.Is<Application>(p => p.Equals(application)),
                            It.Is<ApplicationImage>(p => p.Equals(applicationImage)),
                            It.Is<string>(s => s.Equals(newerImageTag)),
                            It.Is<DeploymentType>( t => t.Equals(DeploymentType.ImageUpdate)),
                            It.Is<string>( s => s == string.Empty),
                            It.IsAny<IReadOnlyDictionary<string,string>>()
                        ),
                        Times.Exactly(createdDeployments)
                        );
                }
                );

            var localContainerMetadataService = MockOf<IContainerImageMetadataService>(
                mock =>
                {
                    mock.Setup(
                        x => x.AddOrUpdate(It.IsAny<ContainerImage>())
                    );
                }
            );

            var job = new ContainerRegistryPollingJob(
                GetLogger<ContainerRegistryPollingJob>(),
                registryClientPool,
                localContainerMetadataService
            );

            // run
            await job.Execute(new ContainerRepositoryPollingContext(imageRepository));
            
            // verify
            VerifyMocks();
        }
    }
}