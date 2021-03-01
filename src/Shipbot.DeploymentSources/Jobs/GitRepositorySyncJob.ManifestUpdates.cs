using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Shipbot.Applications.Models;
using Shipbot.Controller.Core.ApplicationSources.Models;
using YamlDotNet.RepresentationModel;
using Shipbot.Deployments.Models;


namespace Shipbot.Controller.Core.ApplicationSources.Jobs
{
    public partial class GitRepositorySyncJob
    {
        
        class UpdateDeploymentManifestContext : DeploymentManifestSourceSyncContext
        {
            public Deployment Deployment { get; }
            
            public DirectoryInfo GitRepositoryRoot { get; }
            private UpdateDeploymentManifestContext(
                DeploymentManifestSourceTrackingContext trackingContext,
                IReadOnlyDictionary<ApplicationImage, FileInfo> applicationImageToFileMap,
                IReadOnlyDictionary<ApplicationImage, string> manifestTags, 
                Deployment deployment)
                : base(applicationImageToFileMap, manifestTags)
            {
                Deployment = deployment;
                GitRepositoryRoot = new DirectoryInfo(trackingContext.GitRepositoryPath);
            }

            public UpdateDeploymentManifestContext(
                DeploymentManifestSourceTrackingContext trackingContext,

                DeploymentManifestSourceSyncContext syncContext,
                Deployment deployment)
                : this(trackingContext, syncContext.ApplicationImageToFileMapping, syncContext.ManifestTags, deployment)
            {
                
            }

            public static UpdateDeploymentManifestContext Build(
                DeploymentManifestSourceTrackingContext trackingContext,
                Application application, 
                Deployment deployment, 
                IEnumerable<FileInfo> files
            )
            {
                var syncContext = DeploymentManifestSourceSyncContext.Build(application, files);
                return new UpdateDeploymentManifestContext(trackingContext, syncContext, deployment);
            }
        }
        
        private async Task<bool> UpdateDeploymentManifests(
            Repository gitRepository,
            DeploymentUpdate deploymentUpdate,
            UpdateDeploymentManifestContext context,
            string relativePath)
        {
            var yamlUtilities = new YamlUtilities();

            IReadOnlyDictionary<ApplicationImage, FileInfo> imageToFilenameMap = context.ApplicationImageToFileMapping;
            string currentImageTag = context.ManifestTags.GetValueOrDefault(deploymentUpdate.Image) ?? "n/a";

            if (!imageToFilenameMap.TryGetValue(deploymentUpdate.Image, out FileInfo file))
            {
                // TODO: warn that we have an image tag update but no corresponding file
                _log.LogWarning("Update to {Repository} cannot be applied since there isn't matching file.");
                return false;
            }

            _log.LogTrace("Upgrading {Repository} to {NewTag} from {Tag}",
                deploymentUpdate.Image.Repository,
                deploymentUpdate.TargetTag,
                currentImageTag
            );

            var yaml = new YamlStream();
            var filePath = file.FullName;

            yamlUtilities.ReadYamlStream(yaml, filePath);

            var image = deploymentUpdate.Image;

            var doCommit = false;

            foreach (var doc in yaml.Documents)
            {
                var tagInManifest = yamlUtilities.ExtractValueFromDoc(image.TagProperty.Path, doc);
                if (tagInManifest == null) continue;

                if (tagInManifest == deploymentUpdate.TargetTag)
                {
                    _log.LogTrace("Tag for {Repository} matches new tag {NewTag}", image.Repository,
                        deploymentUpdate.TargetTag);
                    continue;
                }

                _log.LogTrace("Setting current-tag for {Repository} to {Tag}", image.Repository,
                    deploymentUpdate.TargetTag);
                yamlUtilities.SetValueInDoc(image.TagProperty.Path, doc, deploymentUpdate.TargetTag);

                doCommit = true;
            }

            if (!doCommit)
                return true;

            yamlUtilities.WriteYamlStream(yaml, filePath);

            // this is GIT requirement: we only work with relative paths in the git repository. 
            _log.LogTrace("Adding {Path} to repository git staging", filePath);
//            var gitFilePath = Path.Combine(relativePath, file.Name);
            var gitFilePath = Path.GetRelativePath(context.GitRepositoryRoot.FullName, file.FullName);
            Commands.Stage(gitRepository, gitFilePath);

            gitRepository.Commit(
                $"[{deploymentUpdate.Application}] Updated deployment for {image.ShortRepository}; {deploymentUpdate.CurrentTag} to {deploymentUpdate.TargetTag}",
                new Signature("deploy-bot", "deploy-bot@riverigaming.com", DateTimeOffset.Now),
                new Signature("deploy-bot", "deploy-bot@riverigaming.com", DateTimeOffset.Now)
            );

            // add delay instruction due to the deployment update notification delivered too fast
            await Task.Delay(500);
            return true;
        }
    }
}