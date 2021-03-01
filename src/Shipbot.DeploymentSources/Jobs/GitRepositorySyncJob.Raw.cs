using System;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="trackingContext"></param>
        /// <param name="gitRepository"></param>
        /// <param name="deployment">Deployment to apply.</param>
        /// <param name="manifest">Deployment manifest source information.</param>
        /// <param name="application">Application that will be updated.</param>
        /// <param name="applicationImage">Image related to this update.</param>
        /// <returns></returns>
        private async Task<bool> ApplyImageUpdateDeployment(
            DeploymentManifestSourceTrackingContext trackingContext,
            Repository gitRepository,
            Deployment deployment,
            RawDeploymentManifest manifest,
            Application application,
            ApplicationImage applicationImage
        )
        {
            var relativePath = manifest.Path;
            var deploymentManifestSourcePath = Path.Combine(trackingContext.GitRepositoryPath, relativePath);

            var files = manifest.Files
                .Select(filename => new FileInfo(Path.Combine(deploymentManifestSourcePath, filename)))
                .ToList();
            
            var updateDeploymentContext = UpdateDeploymentManifestContext.Build(
                trackingContext,
                application, 
                deployment, 
                files);
            
            var deploymentUpdate = new DeploymentUpdate(
                deployment.Id,
                application, 
                applicationImage, 
                deployment.CurrentTag, 
                deployment.TargetTag
            );
                
            return await UpdateDeploymentManifests(
                gitRepository, 
                deploymentUpdate, 
                updateDeploymentContext, 
                relativePath);
        }

        private async Task<bool> ApplyPreviewReleaseToRawDeploymentManifest(Repository gitRepository,
            DeploymentManifestSourceTrackingContext context,
            Application application,
            ApplicationImage applicationImage,
            Deployment deployment,
            RawDeploymentManifest manifest)
        {
            var relativePath = manifest.Path;
            var deploymentManifestSourcePath = Path.Combine(context.GitRepositoryPath, relativePath);

            var copyOperations = new List<(FileInfo source, FileInfo dest)>();

            // build target filelist
            foreach (var file in manifest.Files)
            {
                var sourceFilePath = Path.Combine(deploymentManifestSourcePath, file);
                var sourceFileInfo = new FileInfo(sourceFilePath);

                var targetFilename = $"{Path.GetFileNameWithoutExtension(sourceFileInfo.Name)}-{deployment.InstanceId}{sourceFileInfo.Extension}";
                var targetFilePath = Path.Combine(deploymentManifestSourcePath, targetFilename);
                var targetFileInfo = new FileInfo(targetFilePath);
                
                copyOperations.Add((sourceFileInfo, targetFileInfo));
            }

            // make copies of each file, using the suffix arg
            var instanceAlreadyCreated = false;
            if (copyOperations.Select(x=>x.dest).All(x => !x.Exists))
            {
                // check if all the file exist or not, we come in here when they are all missing
                copyOperations.ForEach( x=>x.source.CopyTo(x.dest.FullName));
            }
            else if (copyOperations.Select(x => x.dest).All(x => x.Exists))
            {
                // all file exist
                instanceAlreadyCreated = true;
            }
            else
            {
                // the fuck, some files exist some not
                // TODO: handle /\
            }

            // build codified representation of the manifest files
            var imageMap = application.Images.ToDictionary(
                x => $"{x.Repository}-{x.TagProperty.Path}"
            );

            var yamlUtilities = new YamlUtilities();
            
            // TODO: load replacement entires from configuration
            var replacementEntries = new Dictionary<string, string>()
            {
                {"image.tag", deployment.TargetTag},
                {"ingress.hosts[0].host", "{instanceid}.{original}"}
            };
            
            var doCommit = false;

            foreach (var file in copyOperations.Select(x=>x.dest))
            {
                var modified = false;
                
                var yamlStream = new YamlStream();

                {
                    await using var stream = file.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
                    yamlUtilities.ReadYamlStream(yamlStream, stream);

                    foreach (var item in replacementEntries)
                    {
                        var currentValue = yamlUtilities.ExtractValueFromDoc(item.Key, yamlStream.Documents.First());
                        if (currentValue == null)
                            continue;

                        var replacementValue = item.Value;
                        replacementValue = replacementValue.Replace("{original}", currentValue);
                        replacementValue = replacementValue.Replace("{instanceid}", deployment.InstanceId);
                        foreach (var param in deployment.Parameters)
                        {
                            replacementValue = replacementValue.Replace($"{{{param.Key}}}", param.Value);
                        }
                        
                        yamlUtilities.SetValueInDoc(item.Key, yamlStream.Documents.First(), replacementValue);
                        modified = true;
                    }
                }

                if (modified)
                {
                    await using var stream = file.Open(FileMode.Create, FileAccess.Write, FileShare.None);
                    yamlUtilities.WriteYamlStream(yamlStream, stream);
                    
                    var gitFilePath = Path.Combine(relativePath, file.Name);
                    Commands.Stage(gitRepository, gitFilePath);

                    doCommit = true;
                }
            }

            if (doCommit)
            {
                if (!instanceAlreadyCreated)
                {
                    gitRepository.Commit(
                        $"[{application.Name}] Created new Preview Release instance ({deployment.InstanceId}) deployment for image '{applicationImage.ShortRepository}:{deployment.TargetTag}'.",
                        new Signature("deploy-bot", "deploy-bot@riverigaming.com", DateTimeOffset.Now),
                        new Signature("deploy-bot", "deploy-bot@riverigaming.com", DateTimeOffset.Now)
                    );    
                }

                else
                {
                    gitRepository.Commit(
                        $"[{application.Name}] Updated preview release deployment for instance '{deployment.InstanceId}' with {applicationImage.ShortRepository}:{deployment.TargetTag}. (From tag '{deployment.CurrentTag}')",
                        new Signature("deploy-bot", "deploy-bot@riverigaming.com", DateTimeOffset.Now),
                        new Signature("deploy-bot", "deploy-bot@riverigaming.com", DateTimeOffset.Now)
                    );
                }
                
            }
            
            return true;
        }
        
        private async Task<bool> SynchronizeRawDeploymentManifestSource(
            Repository gitRepository, 
            DeploymentManifestSourceTrackingContext context, 
            RawDeploymentManifest rawDeploymentManifest)
        {
            var deploymentSourceBasePath = Path.Combine(context.GitRepositoryPath, rawDeploymentManifest.Path);

            var application = _applicationService.GetApplication(context.ApplicationName);
            var imageMap = application.Images.ToDictionary(
                x => $"{x.Repository}-{x.TagProperty.Path}"
            );

            async Task<bool> ApplyImageUpdateDeploymentAndRegister(
                Deployment deployment,
                ApplicationImage image, 
                IDictionary<ApplicationImage, string> imageTags
                )
            {
                var result = await ApplyImageUpdateDeployment(
                    context,
                    gitRepository,
                    deployment,
                    rawDeploymentManifest,
                    application,
                    image
                );
                
                if (result)
                    imageTags[image] = deployment.TargetTag;

                return result;
            }
            
            _log.LogTrace("Beginning to parse value files defined in application source ...");

            var directory = new DirectoryInfo(deploymentSourceBasePath);
            var instanceToFileMapping = new Dictionary<string, List<FileInfo>>();
            
            instanceToFileMapping[string.Empty] = rawDeploymentManifest.Files
                    .Select(file => new FileInfo(Path.Combine(deploymentSourceBasePath, file)))
                    .ToList();

            foreach (var f in rawDeploymentManifest.Files)
            {
                var pattern = $"{Path.GetFileNameWithoutExtension(f)}-*{Path.GetExtension(f)}";
                var files = directory.GetFiles(pattern);
                var instanceIds = files
                    .Select(
                        x => (file: x, nameWithoutExt: Path.GetFileNameWithoutExtension(x.Name))
                    )
                    .Select(
                        pair => (
                            instanceId: pair.nameWithoutExt.Substring(Path.GetFileNameWithoutExtension(f).Length+1),
                            file: pair.file
                            )
                    )
                    .ToList();

                foreach (var item in instanceIds)
                {
                    if (instanceToFileMapping.ContainsKey(item.instanceId))
                    {
                        instanceToFileMapping[item.instanceId].Add(item.file);
                    }
                    else
                    {
                        instanceToFileMapping[item.instanceId] = new List<FileInfo>() {item.file};
                    }
                }
            }

            var syncContextMap = new Dictionary<string, DeploymentManifestSourceSyncContext>();

            foreach (var instanceMapping in instanceToFileMapping)
            {
                syncContextMap[instanceMapping.Key] = DeploymentManifestSourceSyncContext.Build(
                    application,
                    instanceMapping.Value
                );
            }

            _log.LogTrace("Completing parsing value files defined in application source ...");
            
            // start updating files
            var updatedImageTags = new Dictionary<ApplicationImage, string>();
            var manifestsChanged = false;
            Deployment? nextPendingDeployment = null;
            while ((nextPendingDeployment =
                await _deploymentQueueService.GetNextPendingDeploymentUpdate(application)) != null)
            {
                
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
                var deploymentChangedManifests = nextPendingDeployment.Type switch
                {
                    DeploymentType.ImageUpdate => await ApplyImageUpdateDeploymentAndRegister(
                        nextPendingDeployment,
                        image,
                        updatedImageTags),
                    DeploymentType.PreviewRelease => await ApplyPreviewReleaseToRawDeploymentManifest(
                        gitRepository,
                        context,
                        application,
                        image,
                        nextPendingDeployment,
                        rawDeploymentManifest
                        ),
                    _ => throw new ArgumentOutOfRangeException()
                };

                if (deploymentChangedManifests)
                {
                    // imageToTagInManifest[image] = nextPendingDeployment.TargetTag;
                    manifestsChanged = true;
                }
                
                // TODO: rework this so that failures to push get indicated
                await _deploymentService.FinishDeploymentUpdate(
                    nextPendingDeployment.Id,
                    manifestsChanged ? DeploymentStatus.Complete : DeploymentStatus.Failed
                );
            }

            foreach (var syncContextMapping in syncContextMap)
            {
                var instanceId = syncContextMapping.Key;
                var syncContext = syncContextMapping.Value;
                
                foreach (var keyValuePair in syncContext.ManifestTags)
                {
                    
                    if (updatedImageTags.TryGetValue(keyValuePair.Key, out var newlyUpdatedTag))
                    {
                        await _applicationImageInstanceService.SetCurrentTag(application, keyValuePair.Key, instanceId, newlyUpdatedTag);
                    }
                    else
                    {
                        await _applicationImageInstanceService.SetCurrentTag(application, keyValuePair.Key, instanceId, keyValuePair.Value);    
                    }
                }
            }

            return manifestsChanged;
        }
    }
}