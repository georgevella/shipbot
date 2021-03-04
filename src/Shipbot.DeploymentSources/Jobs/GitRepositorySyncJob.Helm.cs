using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Quartz.Util;
using Shipbot.Applications.Models;
using Shipbot.Controller.Core.ApplicationSources.Models;
using Shipbot.Deployments.Models;
using YamlDotNet.RepresentationModel;

namespace Shipbot.Controller.Core.ApplicationSources.Jobs
{
    public partial class GitRepositorySyncJob
    {
        private async Task<bool> SynchronizeHelmApplicationSource(Repository gitRepository,
            DeploymentManifestSourceTrackingContext trackingContext,
            HelmDeploymentManifest helmDeploymentManifest)
        {
            var relativePath = helmDeploymentManifest.Path;
            var deploymentManifestSourcePath = Path.Combine(trackingContext.GitRepositoryPath, relativePath);


            var application = _applicationService.GetApplication(trackingContext.ApplicationName);
            var imageMap = application.Images.ToDictionary(
                x => $"{x.Repository}-{x.TagProperty.Path}"
            );

            // var yamlUtilities = new YamlUtilities();

            // build map of images -> yaml file that defines them and image -> current tag
            _log.LogTrace("Beginning to parse value files defined in application source ...");

            var syncContext = DeploymentManifestSourceSyncContext.Build(
                application,
                helmDeploymentManifest.ValuesFiles
                    .Select(file => new FileInfo(Path.Combine(deploymentManifestSourcePath, file)))
                    .ToList()
            );

            _log.LogTrace("Completing parsing value files defined in application source ...");

            // start updating files
            var updatedImageTags = new Dictionary<ApplicationImage, string>();

            var manifestsChanged = false;
            Deployment? nextPendingDeployment = null;
            while ((nextPendingDeployment =
                await _deploymentQueueService.GetNextPendingDeploymentUpdate(application)) != null)
            {

                var updateDeploymentManifestContext =
                    new UpdateDeploymentManifestContext(trackingContext, syncContext, nextPendingDeployment);
                
                await _deploymentService.ChangeDeploymentUpdateStatus(
                    nextPendingDeployment.Id,
                    DeploymentStatus.Starting
                );
                _log.LogTrace("Executing pending deployment update ...");

                await _deploymentService.ChangeDeploymentUpdateStatus(
                    nextPendingDeployment.Id,
                    DeploymentStatus.UpdatingManifests
                );

                // build a deployment update object
                var image = imageMap[$"{nextPendingDeployment.ImageRepository}-{nextPendingDeployment.UpdatePath}"];
                // send notification out
                var deploymentUpdate = new DeploymentUpdate(
                    nextPendingDeployment.Id,
                    application,
                    image,
                    nextPendingDeployment.CurrentTag,
                    nextPendingDeployment.TargetTag
                );

                manifestsChanged = await UpdateDeploymentManifests(gitRepository,
                    deploymentUpdate,
                    updateDeploymentManifestContext,
                    relativePath);

                if (manifestsChanged)
                {
                    updatedImageTags[image] = nextPendingDeployment.TargetTag;
                }

                // TODO: rework this so that failures to push get indicated
                await _deploymentService.FinishDeploymentUpdate(
                    nextPendingDeployment.Id,
                    manifestsChanged ? DeploymentStatus.Complete : DeploymentStatus.Failed
                );
            }

            if (!manifestsChanged)
            {
                // we don't have a deployment, ensure application manifest is up to date with latest image tags

            }
            
            foreach (var keyValuePair in syncContext.ManifestTags)
            {
                if (updatedImageTags.TryGetValue(keyValuePair.Key, out var newlyUpdatedTag))
                {
                    await _applicationImageInstanceService.SetCurrentTagForPrimary(application, keyValuePair.Key, newlyUpdatedTag);
                }
                else
                {
                    await _applicationImageInstanceService.SetCurrentTagForPrimary(application, keyValuePair.Key, keyValuePair.Value);    
                }
            }

            return manifestsChanged;
        }
    }
}