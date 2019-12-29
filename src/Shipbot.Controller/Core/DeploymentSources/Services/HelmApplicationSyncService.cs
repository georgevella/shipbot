//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Threading.Tasks;
//using LibGit2Sharp;
//using Microsoft.Extensions.Logging;
//using Quartz.Util;
//using Shipbot.Controller.Core.Deployments;
//using Shipbot.Controller.Core.Deployments.Models;
//using Shipbot.Controller.Core.DeploymentSources.Models;
//using Shipbot.Controller.Core.Models;
//using YamlDotNet.RepresentationModel;
//
//namespace Shipbot.Controller.Core.DeploymentSources.Services
//{
//    public class HelmApplicationSyncService : IApplicationSourceSyncService 
//    {
//        private readonly ILogger<HelmApplicationSyncService> _log;
//        private readonly IDeploymentService _deploymentService;
//
//        public HelmApplicationSyncService(
//            ILogger<HelmApplicationSyncService> log,
//            IDeploymentService deploymentService
//            )
//        {
//            _log = log;
//            _deploymentService = deploymentService;
//        }
//        
//        public async Task<DeploymentSourceMetadata> BuildApplicationSourceDetails(
//            ApplicationSourceTrackingContext context,
//            HelmApplicationSource helmApplicationSource
//            )
//        {
//            var relativePath = helmApplicationSource.Path;
//            var applicationSourcePath = Path.Combine(context.GitRepositoryPath.FullName, relativePath);
//
//            var application = context.Application;
//            var yamlUtilities = new YamlUtilities();
//
//            // build map of images -> yaml file that defines them and image -> current tag
//            _log.LogInformation("Beginning to parse value files defined in application source ...");
//
//            var imageToFilenameMap = new Dictionary<Image, FileInfo>();
//            var imageToTagInManifest = new Dictionary<Image, string>();
//
//            foreach (var file in helmApplicationSource.ValuesFiles)
//            {
//                var yaml = new YamlStream();
//                var filePath = Path.Combine(applicationSourcePath, file);
//                yamlUtilities.ReadYamlStream(yaml, filePath);
//
//                foreach (var doc in yaml.Documents)
//                {
//                    foreach (var image in context.Environment.Images)
//                    {
//                        var tagInManifest = yamlUtilities.ExtractValueFromDoc(image.TagProperty.Path, doc);
//
//                        if (tagInManifest == null)
//                            continue;
//
//                        if (imageToFilenameMap.ContainsKey(image))
//                        {
//                            // TODO: handle situation where multiple files define the same image tag (ERROR and warn user)
//                        }
//
//                        imageToFilenameMap[image] = new FileInfo(filePath);
//                        imageToTagInManifest[image] = tagInManifest;
//                    }
//                }
//            }
//            
//            _log.LogInformation("Completing parsing value files defined in application source ...");
//            
//            return new DeploymentSourceMetadata(
//                imageToTagInManifest, 
//                imageToFilenameMap,
//                context.GitRepositoryPath
//            );
//        }
//
//        public async Task ApplyDeploymentUpdates(
//            DeploymentSourceMetadata deploymentSourceMetadata,
//            DeploymentUpdate deploymentUpdate)
//        {
//            var manifestsChanged = false;
////            DeploymentUpdate deploymentUpdate = null;
////            while ((deploymentUpdate = await _deploymentService.GetNextPendingDeploymentUpdate(application)) != null) 
//            {
//                _log.LogInformation("Executing pending deployment update ...");
//                
//                await _deploymentService.ChangeDeploymentUpdateStatus(
//                    deploymentUpdate,
//                    DeploymentUpdateStatus.UpdatingManifests
//                );
//
//                if (!deploymentSourceMetadata.Files.TryGetValue(deploymentUpdate.Image, out FileInfo file))
//                {
//                    // TODO: warn that we have an image tag update but no corresponding file
//                    _log.LogWarning("Update to {Repository} cannot be applied since there isn't matching file.");
//                    return;
//                }
//
//                manifestsChanged = await UpdateDeploymentManifests(
//                    deploymentSourceMetadata, 
//                    deploymentUpdate, 
//                    file,
//                    deploymentSourceMetadata.Tags.TryGetAndReturn(deploymentUpdate.Image) ?? "n/a"
//                    );
//
////                if (manifestsChanged)
////                {
////                    imageToTagInManifest[deploymentUpdate.Image] = deploymentUpdate.TargetTag;
////                }
//                
//                await _deploymentService.FinishDeploymentUpdate(
//                    deploymentUpdate,
//                    manifestsChanged ? DeploymentUpdateStatus.Complete : DeploymentUpdateStatus.Failed
//                );
//            }
//        }
//        
//        private async Task<bool> UpdateDeploymentManifests(
//            DeploymentSourceMetadata applicationSourceMetadata,
//            DeploymentUpdate deploymentUpdate,
//            FileSystemInfo file,
//            string currentImageTag)
//        {
////            if (!imageToFilenameMap.TryGetValue(deploymentUpdate.Image, out FileInfo file))
////            {
////                // TODO: warn that we have an image tag update but no corresponding file
////                _log.LogWarning("Update to {Repository} cannot be applied since there isn't matching file.");
////                return false;
////            }
//
//            _log.LogInformation("Upgrading {Repository} to {NewTag} from {Tag}",
//                deploymentUpdate.Image.Repository,
//                deploymentUpdate.TargetTag,
//                currentImageTag
//            );
//            
//            var yamlUtilities = new YamlUtilities();
//
//            var yaml = new YamlStream();
//            var filePath = file.FullName;
//            
//            yamlUtilities.ReadYamlStream(yaml, filePath);
//
//            var image = deploymentUpdate.Image;
//
//            var doCommit = false;
//
//            foreach (var doc in yaml.Documents)
//            {
//                var tagInManifest = yamlUtilities.ExtractValueFromDoc(image.TagProperty.Path, doc);
//                if (tagInManifest == null) continue;
//
//                if (tagInManifest == deploymentUpdate.TargetTag)
//                {
//                    _log.LogInformation("Tag for {Repository} matches new tag {NewTag}", image.Repository, deploymentUpdate.TargetTag);
//                    continue;
//                }
//
//                _log.LogInformation("Setting current-tag for {Repository} to {Tag}", image.Repository,
//                    deploymentUpdate.TargetTag);
//                yamlUtilities.SetValueInDoc(image.TagProperty.Path, doc, deploymentUpdate.TargetTag);
//
//                doCommit = true;
//            }
//
//            if (!doCommit)
//                return true;
//
//            yamlUtilities.WriteYamlStream(yaml, filePath);
//
//            using (var gitRepository = new Repository(applicationSourceMetadata.GitRepositoryPath.FullName))
//            {
//
//                var gitFilePath =
//                    Path.GetRelativePath(applicationSourceMetadata.GitRepositoryPath.FullName, file.FullName);
//                _log.LogInformation("Adding {Path} to repository staging", gitFilePath);
//                Commands.Stage(gitRepository, gitFilePath);
//
//                gitRepository.Commit(
//                    $"Updated deployment for {image.Repository} to version with tag {deploymentUpdate.TargetTag}",
//                    new Signature("deploy-bot", "deploy-bot@riverigaming.com", DateTimeOffset.Now),
//                    new Signature("deploy-bot", "deploy-bot@riverigaming.com", DateTimeOffset.Now)
//                );
//            }
//
//            // add delay instruction due to the deployment update notification delivered too fast
//            await Task.Delay(500);
//            return true;
//        }
//
//    }
//}