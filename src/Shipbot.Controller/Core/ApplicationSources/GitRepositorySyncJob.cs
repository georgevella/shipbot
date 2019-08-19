using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Util;
using Shipbot.Controller.Core.Apps;
using Shipbot.Controller.Core.Models;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace Shipbot.Controller.Core.ApplicationSources
{
    [DisallowConcurrentExecution]
    public class GitRepositorySyncJob : IJob
    {
        private readonly ILogger<GitRepositorySyncJob> _log;
        private readonly IApplicationService _applicationService;

        public GitRepositorySyncJob(
            ILogger<GitRepositorySyncJob> log,
            IApplicationService applicationService
        )
        {
            _log = log;
            _applicationService = applicationService;
        }

        public async Task Execute(IJobExecutionContext jobExecutionContext)
        {
            // TODO: report failures to Application model

            var data = jobExecutionContext.JobDetail.JobDataMap;
            var context = (ApplicationSourceTrackingContext) data["Context"];

            using (_log.BeginScope(new Dictionary<string, object>
            {
                {"Application", context.Application.Name},
                {"Ref", context.Application.Source.Repository.Ref},
                {"Path", context.Application.Source.Path}
            }))
            {
                var repository = context.Application.Source.Repository;

                // TODO: improve this to not have passwords in memory / use SecureStrings
                var credentials = (UsernamePasswordGitCredentials) repository.Credentials;

                var gitRepository = new LibGit2Sharp.Repository(context.GitRepositoryPath);

                var branchNames = gitRepository.Branches.Select(x => x.CanonicalName).ToList();

                var branch = gitRepository.Branches.FirstOrDefault(b => b.FriendlyName == repository.Ref);
                var originBranch =
                    gitRepository.Branches.FirstOrDefault(b => b.FriendlyName == $"origin/{repository.Ref}");
                if (branch != null || originBranch != null)
                {
                    if (branch == null)
                    {
                        // checkout branch from origin and make sure we are tracking the remote branchb
                        branch = gitRepository.CreateBranch(repository.Ref, originBranch.Tip);
                        branch = gitRepository.Branches.Update(branch,
                            b => b.TrackedBranch = originBranch.CanonicalName
                        );
                    }

                    // we already have a local copy of the branch, make sure it's the current head (if not switch to it)
                    if (branch.Tip != gitRepository.Head.Tip)
                    {
                        // switch to selected branch
                        _log.LogInformation("Switching from branch {currentBranch} to {requestedBranch}",
                            gitRepository.Head.CanonicalName, branch.CanonicalName);
                        Commands.Checkout(gitRepository, branch);
                    }

                    var currentHash = gitRepository.Head.Tip.Sha;

                    _log.LogInformation("Fetching latest sources for {branch} [{currentHash}] ...",
                        branch.CanonicalName, currentHash);

                    // TODO maybe this needs to be a fetch
                    Commands.Pull(gitRepository,
                        new Signature("rig-deploy-bot", "devops@riverigaming.com", DateTimeOffset.Now),
                        new PullOptions()
                        {
                            FetchOptions = new FetchOptions()
                            {
                                Prune = true,
                                CredentialsProvider = (url, fromUrl, types) => new UsernamePasswordCredentials()
                                {
                                    Username = credentials.Username,
                                    Password = credentials.Password
                                }
                            },
                            MergeOptions = new MergeOptions()
                            {
                                FastForwardStrategy = FastForwardStrategy.FastForwardOnly
                            }
                        });

                    if (gitRepository.Head.Tip.Sha != currentHash)
                    {
                        _log.LogInformation("Branch changed, triggered application refresh");
                    }
                }
                else
                {
                    var tag = gitRepository.Tags[repository.Ref];
                    if (tag != null)
                    {
                        var currentHash = tag.Target.Sha;

                        Commands.Fetch(
                            gitRepository,
                            "origin",
                            Enumerable.Empty<string>(),
                            new FetchOptions()
                            {
                                Prune = true,
                                CredentialsProvider = (url, fromUrl, types) => new UsernamePasswordCredentials()
                                {
                                    Username = credentials.Username,
                                    Password = credentials.Password
                                }
                            },
                            null);


                        tag = gitRepository.Tags[repository.Ref];

                        if (currentHash != tag.Target.Sha)
                        {
                            _log.LogInformation("Tag hash changed, triggered application refresh.");
                        }
                    }
                    else
                    {
                        // its a git commit hash
                        var currentHash = gitRepository.Head.Tip.Sha;

                        if (currentHash != repository.Ref)
                        {
                            _log.LogInformation(
                                $"Current hash [{currentHash}] is not requested hash [{repository.Ref}], git checkout of commit will be triggered.");

                            Commands.Checkout(gitRepository, repository.Ref);
                        }
                    }
                }

                if (context.Application.Source is HelmApplicationSource helmApplicationSource)
                {
                    if (SynchronizeHelmApplicationSource(gitRepository, context, helmApplicationSource) &&
                        context.Application.AutoDeploy)
                    {
                        _log.LogInformation("Pushing repository changes for {application}", context.Application);

                        gitRepository.Network.Push(branch, new PushOptions()
                        {
                            CredentialsProvider = (url, fromUrl, types) => new UsernamePasswordCredentials()
                            {
                                Username = credentials.Username,
                                Password = credentials.Password
                            }
                        });
                    }
                }
            }
        }

        private bool SynchronizeHelmApplicationSource(
            Repository gitRepository,
            ApplicationSourceTrackingContext context,
            HelmApplicationSource helmApplicationSource)
        {
            var relativePath = helmApplicationSource.Path;
            var applicationSourcePath = Path.Combine(context.GitRepositoryPath, relativePath);

            var application = context.Application;
            var yamlUtilities = new YamlUtilities();

            // build map of images -> yaml file that defines them and image -> current tag
            _log.LogInformation("Beginning to parse value files defined in application source ...");

            var imageToFilenameMap = new Dictionary<Image, string>();
            var imageToTagInManifest = new Dictionary<Image, string>();

            foreach (var file in helmApplicationSource.ValuesFiles)
            {
                var yaml = new YamlStream();
                var filePath = Path.Combine(applicationSourcePath, file);
                yamlUtilities.ReadYamlStream(yaml, filePath);

                foreach (var doc in yaml.Documents)
                {
                    foreach (var image in context.Application.Images)
                    {
                        var tagInManifest = yamlUtilities.ExtractValueFromDoc(image.TagProperty.Path, doc);

                        if (tagInManifest == null)
                            continue;

                        if (imageToFilenameMap.ContainsKey(image))
                        {
                            // TODO: handle situation where multiple files define the same image tag (ERROR and warn user)
                        }

                        imageToFilenameMap[image] = file;
                        imageToTagInManifest[image] = tagInManifest;
                    }
                }
            }

            // start updating files
            var doPush = false;

            var updates = new List<DeploymentUpdate>();
            var queue = _applicationService.BeginApplicationSync(application);
            foreach (var targetImageTag in queue)
            {
                _applicationService.UpdateDeploymentUpdateState(
                    application, 
                    targetImageTag,
                    DeploymentUpdateStatus.UpdatingManifests
                    );
                
                if (!imageToFilenameMap.TryGetValue(targetImageTag.Image, out string file))
                {
                    // TODO: warn that we have an image tag update but no corresponding file
                    _log.LogWarning("Update to {image} cannot be applied since there isn't matching file.");
                    continue;
                }

                _log.LogInformation("Upgrading {image} to {newTag} from {currentTag}",
                    targetImageTag.Image.Repository,
                    targetImageTag.Tag,
                    imageToTagInManifest.TryGetAndReturn(targetImageTag.Image) ?? "n/a"
                );

                var yaml = new YamlStream();
                var filePath = Path.Combine(applicationSourcePath, file);

                yamlUtilities.ReadYamlStream(yaml, filePath);

                var image = targetImageTag.Image;

                var doCommit = false;

                foreach (var doc in yaml.Documents)
                {
                    var tagInManifest = yamlUtilities.ExtractValueFromDoc(image.TagProperty.Path, doc);
                    if (tagInManifest == null) continue;

                    if (tagInManifest == targetImageTag.Tag)
                    {
                        _log.LogInformation("Tag for {image} matches new tag {newTag}", image.Repository, targetImageTag.Tag);
                        continue;
                    }

                    _log.LogInformation("Setting current-tag for {image} to {tag}", image.Repository,
                        targetImageTag.Tag);
                    yamlUtilities.SetValueInDoc(image.TagProperty.Path, doc, targetImageTag.Tag);

                    doCommit = true;
                }

                if (!doCommit) 
                    continue;
                
                yamlUtilities.WriteYamlStream(yaml, filePath);

                _log.LogInformation("Adding {filePath} to repository staging", filePath);
                var gitFilePath = Path.Combine(relativePath, file);
                Commands.Stage(gitRepository, gitFilePath);

                gitRepository.Commit(
                    $"Updated deployment for {image.Repository} to version with tag {targetImageTag.Tag}",
                    new Signature("deploy-bot", "deploy-bot@riverigaming.com", DateTimeOffset.Now),
                    new Signature("deploy-bot", "deploy-bot@riverigaming.com", DateTimeOffset.Now)
                );
                    
                // keep note of the updated tag
                imageToTagInManifest[image] = targetImageTag.Tag;
                updates.Add(targetImageTag);
                
                _applicationService.UpdateDeploymentUpdateState(application, targetImageTag,
                    DeploymentUpdateStatus.Synchronized);

                doPush = true;
            }

            // ensure application state contains correct image tags
            _applicationService.EndApplicationSync(application,
                updates,
                imageToTagInManifest.Select(x => (x.Key, x.Value)).ToList()
            );

            /////

//            foreach (var file in helmApplicationSource.ValuesFiles)
//            {
//                var fileUpdated = false;
//                
//                var yaml = new YamlStream();
//                var filePath = Path.Combine(applicationSourcePath, file);
//                yamlUtilities.ReadYamlStream(yaml, filePath);
//
//                foreach (var doc in yaml.Documents)
//                {
//                    foreach (var image in context.Application.Images)
//                    {
//                        var tagInManifest = yamlUtilities.ExtractValueFromDoc(image.TagProperty.Path, doc);
//
//                        if (tagInManifest == null) continue;
//
//                        if (!targetImageTags.TryGetValue(image, out var tagInContext))
//                        {
//                            _log.LogInformation("Detected a new image, {image} with tag {tag}, in manifests.", image.Repository, tagInManifest);
//                            _applicationService.SetCurrentImageTag(application, image, tagInManifest);
//                            continue;
//                        }
//                        
//                        if (tagInContext == tagInManifest)
//                        {
//                            _log.LogInformation("Current tag in manifest for {image} did not change ({tag})", image.Repository, tagInManifest);
//                                
//                        }
//                        else
//                        {
//                            _log.LogInformation("Setting current-tag for {image} to {tag}", image.Repository, tagInContext);
//                            yamlUtilities.SetValueInDoc(image.TagProperty.Path, doc, tagInContext);
//                            
//                            // TODO: improve this so that current image tags are set after file is saved and committed
//                            _applicationService.SetCurrentImageTag(application, image, tagInManifest);
//                            fileUpdated = true;
//                        }
//                    }
//                }
//
//                if (!fileUpdated) 
//                    continue;
//                
//                yamlUtilities.WriteYamlStream(yaml, filePath);
//
//                var gitFilePath = Path.Combine(relativePath, file);
//                _log.LogInformation("Adding {filePath} to repository staging", gitFilePath);
//                    
//                Commands.Stage(gitRepository, gitFilePath);
//
//                doCommit = true;
//            }

            _log.LogInformation("Completing parsing value files defined in application source ...");

            return doPush;
        }
    }
}