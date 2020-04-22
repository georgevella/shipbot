using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Orleans;
using Quartz.Util;
using Shipbot.Controller.Core.Apps.GrainState;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Configuration.ApplicationSources;
using Shipbot.Controller.Core.Deployments.GrainKeys;
using Shipbot.Controller.Core.Deployments.GrainState;
using Shipbot.Controller.Core.Deployments.Models;
using Shipbot.Controller.Core.DeploymentSources.Exceptions;
using Shipbot.Controller.Core.DeploymentSources.GrainState;
using Shipbot.Controller.Core.DeploymentSources.Models;
using Shipbot.Controller.Core.Models;
using Shipbot.Controller.Core.Utilities;
using YamlDotNet.RepresentationModel;
using ApplicationSourceRepository = Shipbot.Controller.Core.DeploymentSources.Models.ApplicationSourceRepository;

namespace Shipbot.Controller.Core.DeploymentSources.Grains
{
    public class HelmDeploymentSourceGrain : GitBasedDeploymentSourceGrain<HelmApplicationSource>, IHelmDeploymentSourceGrain
    {
        private readonly ILogger<HelmDeploymentSourceGrain> _log;

        public HelmDeploymentSourceGrain(
            ILogger<HelmDeploymentSourceGrain> log
            ) : base(log)
        {
            _log = log;
        }

        public override Task OnActivateAsync()
        {
            using var _ = _log.BeginShipbotLogScope(ApplicationEnvironmentKey.Parse(this.GetPrimaryKeyString()));
            
            if (State.SecretFiles == null) State.SecretFiles = new List<string>();
            if (State.ValuesFiles == null) State.ValuesFiles = new List<string>();
            if (State.Repository == null) State.Repository = new ApplicationSourceRepository();

            return base.OnActivateAsync();
        }


        public override async Task Configure(
            ApplicationSourceSettings applicationSourceSettings,
            ApplicationEnvironmentKey applicationEnvironmentKey
        )
        {
            using var _ = _log.BeginShipbotLogScope(ApplicationEnvironmentKey.Parse(this.GetPrimaryKeyString()));
            
            if (applicationSourceSettings == null) throw new ArgumentNullException(nameof(applicationSourceSettings));
            
            await base.Configure(applicationSourceSettings, applicationEnvironmentKey);

            State.SecretFiles = applicationSourceSettings.Helm.Secrets;
            State.ValuesFiles = applicationSourceSettings.Helm.ValueFiles;
        }
        
        public override async Task Refresh()
        {
            using var logScope = _log.BeginShipbotLogScope(ApplicationEnvironmentKey.Parse(this.GetPrimaryKeyString()));
            
            var relativePath = State.Path;
            var applicationSourcePath = Path.Combine(this.CheckoutDirectory.FullName, relativePath);
            
            var yamlUtilities = new YamlUtilities();

            // build map of images -> yaml file that defines them and image -> current tag
            _log.LogInformation("Beginning to parse value files defined in application source ...");

            var environmentGrain = GrainFactory.GetEnvironment(State.ApplicationEnvironment);

            var imageMetadataFromFile = new Dictionary<string, (FileInfo, string)>();
    
            foreach (var file in State.ValuesFiles)
            {
                var yaml = new YamlStream();
                var filePath = Path.Combine(applicationSourcePath, file);
                yamlUtilities.ReadYamlStream(yaml, filePath);

                foreach (var doc in yaml.Documents)
                {
                    foreach (var image in await environmentGrain.GetImages())
                    {
                        var tagInManifest = yamlUtilities.ExtractValueFromDoc(image.ImageTagValuePath, doc);

                        if (tagInManifest == null)
                            continue;

                        if (imageMetadataFromFile.ContainsKey(image.ImageTagValuePath))
                        {
                            // TODO: handle situation where multiple files define the same image tag (ERROR and warn user)
                        }

                        imageMetadataFromFile[image.ImageTagValuePath] = (new FileInfo(filePath), tagInManifest);
                    }
                }
            }

            State.Metadata.ImageTags = imageMetadataFromFile.Select(
                    x => new DeploymentSourceValuePathMetadata(
                        x.Key,            // valuepath
                        x.Value.Item2,    // value
                        x.Value.Item1.FullName // filename
                    )
                )
                .ToHashSet(new DeploymentSourceValuePathEqualityComparer());

            await WriteStateAsync();
            
            _log.LogInformation("Completing parsing value files defined in application source ...");
        }

        public override async Task<DeploymentSourceChangeResult> ApplyDeploymentAction(DeploymentSourceChange deploymentSourceChange)
        {
            using var logScope = _log.BeginShipbotLogScope(ApplicationEnvironmentKey.Parse(this.GetPrimaryKeyString()));
            
            // var deploymentGrain = GrainFactory.GetDeploymentGrain(State.ApplicationEnvironment,
            //     deploymentUpdate.Image, deploymentUpdate.TargetTag);
            //var deploymentUpdateGrain = GrainFactory.GetDeploymentActionGrain(deploymentActionKey);
            var environmentGrain = GrainFactory.GetEnvironment(State.ApplicationEnvironment);

            var manifestsChanged = false;
//            DeploymentUpdate deploymentUpdate = null;
//            while ((deploymentUpdate = await _deploymentService.GetNextPendingDeploymentUpdate(application)) != null) 
            _log.LogInformation("Executing pending deployment update ...");

            // await deploymentUpdateGrain.SetStatus(
            //     DeploymentActionStatus.UpdatingManifests
            // );

            //var image = await deploymentUpdateGrain.GetImage();

            foreach (var deploymentSourceValuePathMetadata in State.Metadata.ImageTags)
            {
                if (deploymentSourceValuePathMetadata.ValuePath.Equals(deploymentSourceChange.ValuePath))
                {
                    var currentTags = await environmentGrain.GetCurrentImageTags();
                    
                    var pair = currentTags.First(x =>
                        x.Key.ImageTagValuePath.Equals(deploymentSourceChange.ValuePath));

                    manifestsChanged = await UpdateDeploymentManifests(
                        pair.Key,
                         deploymentSourceChange.NewValue,
                        new FileInfo(deploymentSourceValuePathMetadata.File),
                        pair.Value
                    );

                    break;
                }
            }


//             if (!State.Metadata.ImageTags.TryGetValue(deploymentSourceChange.ValuePath, out var tagSourceFile))
//             {
//                 // TODO: warn that we have an image tag update but no corresponding file
//                 _log.LogWarning("Update to {Repository} cannot be applied since there isn't matching file.");
//                 return;
//             }
//
//             
//             if (!currentTags.TryGetValue(image, out string tag))
//             {
//                 tag = "n/a";
//             }
//
//
//
                // if (manifestsChanged)
                // {
                //     imageToTagInManifest[deploymentUpdate.Image] = deploymentUpdate.TargetTag;
                // }
                
                return new DeploymentSourceChangeResult(true, deploymentSourceChange);
        }
        
        private async Task<bool> UpdateDeploymentManifests(
            ApplicationEnvironmentImageMetadata image,
            string targetTag,
            FileSystemInfo file,
            string currentImageTag)
        {
            using var logScope = _log.BeginShipbotLogScope(ApplicationEnvironmentKey.Parse(this.GetPrimaryKeyString()));
            
//            if (!imageToFilenameMap.TryGetValue(deploymentUpdate.Image, out FileInfo file))
//            {
//                // TODO: warn that we have an image tag update but no corresponding file
//                _log.LogWarning("Update to {Repository} cannot be applied since there isn't matching file.");
//                return false;
//            }

            _log.LogInformation("Upgrading {Repository} to {NewTag} from {Tag}",
                image.Repository,
                targetTag,
                currentImageTag
            );
            
            var yamlUtilities = new YamlUtilities();

            var yaml = new YamlStream();
            var filePath = file.FullName;
            
            yamlUtilities.ReadYamlStream(yaml, filePath);

            var doCommit = false;

            foreach (var doc in yaml.Documents)
            {
                var tagInManifest = yamlUtilities.ExtractValueFromDoc(image.ImageTagValuePath, doc);
                if (tagInManifest == null) continue;

                if (tagInManifest == targetTag)
                {
                    _log.LogInformation("Tag for {Repository} matches new tag {NewTag}", image.Repository, targetTag);
                    continue;
                }

                _log.LogInformation("Setting current-tag for {Repository} to {Tag}", image.Repository,
                    targetTag);
                yamlUtilities.SetValueInDoc(image.ImageTagValuePath, doc, targetTag);

                doCommit = true;
            }

            if (!doCommit)
                return true;

            yamlUtilities.WriteYamlStream(yaml, filePath);

            using (var gitRepository = new Repository(CheckoutDirectory.FullName))
            {

                var gitFilePath =
                    Path.GetRelativePath(CheckoutDirectory.FullName, file.FullName);
                _log.LogInformation("Adding {Path} to repository staging", gitFilePath);
                Commands.Stage(gitRepository, gitFilePath);

                gitRepository.Commit(
                    $"Updated deployment for {image.Repository} to version with tag {targetTag}",
                    new Signature("deploy-bot", "deploy-bot@riverigaming.com", DateTimeOffset.Now),
                    new Signature("deploy-bot", "deploy-bot@riverigaming.com", DateTimeOffset.Now)
                );

                try
                {
                    var branch = gitRepository.Branches[State.Repository.Ref];
                    gitRepository.Network.Push(branch);
                }
                catch (Exception e)
                {
                    _log.LogWarning("Failed to push to branch '{branch}'", State.Repository.Ref);
                    var remote = gitRepository.Network.Remotes["origin"];
                    
                    var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                    Commands.Fetch(gitRepository, remote.Name, refSpecs, null, "");

                    gitRepository.Rebase.Start(gitRepository.Branches[State.Repository.Ref],
                        gitRepository.Branches[$"origin/{State.Repository.Ref}"],
                        gitRepository.Branches[$"origin/{State.Repository.Ref}"],
                        new Identity("shipbot", "shipbot@email"), new RebaseOptions()
                        {

                        });
                }

            }

            // add delay instruction due to the deployment update notification delivered too fast
            await Task.Delay(500);
            return true;
        }
    }
}