using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Util;
using Shipbot.Applications;
using Shipbot.Controller.Core.ApplicationSources.Models;
using Shipbot.Deployments;
using Shipbot.JobScheduling;
using Shipbot.Models;
using YamlDotNet.RepresentationModel;

namespace Shipbot.Controller.Core.ApplicationSources.Jobs
{
    [DisallowConcurrentExecution]
    public class GitRepositorySyncJob : BaseJobWithData<ApplicationSourceTrackingContext>
    {
        private readonly ILogger<GitRepositorySyncJob> _log;
        private readonly IApplicationService _applicationService;
        private readonly IDeploymentService _deploymentService;
        private readonly IDeploymentQueueService _deploymentQueueService;

        public GitRepositorySyncJob(
            ILogger<GitRepositorySyncJob> log,
            IApplicationService applicationService,
            IDeploymentService deploymentService,
            IDeploymentQueueService deploymentQueueService
        )
        {
            _log = log;
            _applicationService = applicationService;
            _deploymentService = deploymentService;
            _deploymentQueueService = deploymentQueueService;
        }

        protected override async Task Execute(ApplicationSourceTrackingContext context)
        {
            using (_log.BeginScope(new Dictionary<string, object>
            {
                {"Application", context.ApplicationName},
                {"Ref", context.ApplicationSource.Repository.Ref},
                {"Path", context.ApplicationSource.Path}
            }))
            {
                var repository = context.ApplicationSource.Repository;

                // TODO: improve this to not have passwords in memory / use SecureStrings
                var credentials = (UsernamePasswordGitCredentials) repository.Credentials;
                
                _log.LogInformation("Removing local copy of git repository",
                    repository.Uri,
                    context.GitRepositoryPath);
                
                if (Directory.Exists(context.GitRepositoryPath))
                {
                    Directory.Delete(context.GitRepositoryPath, true);
                }

                _log.LogInformation("Cloning {Repository} into {Path}",
                    repository.Uri,
                    context.GitRepositoryPath);

                Repository.Clone(
                    repository.Uri.ToString(),
                    context.GitRepositoryPath,
                    new CloneOptions()
                    {
                        CredentialsProvider = (url, fromUrl, types) => new UsernamePasswordCredentials()
                        {
                            Username = credentials.Username,
                            Password = credentials.Password
                        }
                    });
                
                using var gitRepository = new Repository(context.GitRepositoryPath);

                
                var branch = CheckoutDeploymentManifest(gitRepository, repository, credentials);
                
                // TODO: handle scenario when we are tracking a git commit or a tag

                if (context.ApplicationSource is HelmApplicationSource helmApplicationSource)
                {
                    if (await SynchronizeHelmApplicationSource(gitRepository, context, helmApplicationSource) &&
                        context.AutoDeploy)
                    {
                        int attempt = 3;

                        while (attempt > 0)
                        {
                            _log.LogInformation("Pushing repository changes for {application}", context.ApplicationName);
                        
                            try
                            {
                                gitRepository.Network.Push(branch, new PushOptions()
                                {
                                    CredentialsProvider = (url, fromUrl, types) => new UsernamePasswordCredentials()
                                    {
                                        Username = credentials.Username,
                                        Password = credentials.Password
                                    }
                                });

                                break;
                            }
                            catch (NonFastForwardException e)
                            {
                                _log.LogInformation("Remote has newer commits, re-fetching");
                                var remote = gitRepository.Network.Remotes["origin"];
                                var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                                Commands.Fetch(gitRepository, remote.Name, refSpecs, null, "");
                            
                                _log.LogInformation("Rebasing on remote branch prior to trying to re-push changes ...");
                                var upstream = gitRepository.Head.TrackedBranch;
                                gitRepository.Rebase.Start(gitRepository.Head, upstream, upstream, new Identity("deploy-bot", "deploy-bot@riverigaming.com"), new RebaseOptions()
                                {
                                    FileConflictStrategy = CheckoutFileConflictStrategy.Theirs,
                                });

                                
                                --attempt;
                            }
                        }
                        
                    }
                }
            }
        }

        private Branch CheckoutDeploymentManifest(Repository gitRepository, ApplicationSourceRepository repository,
            UsernamePasswordGitCredentials credentials)
        {
            _log.LogInformation("CheckoutDeploymentManifest ...");
            
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

            return branch;
        }

        private async Task<bool> SynchronizeHelmApplicationSource(Repository gitRepository,
            ApplicationSourceTrackingContext context,
            HelmApplicationSource helmApplicationSource)
        {
            var relativePath = helmApplicationSource.Path;
            var applicationSourcePath = Path.Combine(context.GitRepositoryPath, relativePath);


            var application = _applicationService.GetApplication(context.ApplicationName);
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
                    foreach (var image in application.Images)
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
            
            _log.LogInformation("Completing parsing value files defined in application source ...");

            // start updating files
            var manifestsChanged = false;
            DeploymentUpdate deploymentUpdate = null;
            while ((deploymentUpdate = await _deploymentQueueService.GetNextPendingDeploymentUpdate(application)) != null)
            {
                await _deploymentService.ChangeDeploymentUpdateStatus(deploymentUpdate,
                    DeploymentUpdateStatus.Starting);
                _log.LogInformation("Executing pending deployment update ...");
                
                await _deploymentService.ChangeDeploymentUpdateStatus(
                    deploymentUpdate,
                    DeploymentUpdateStatus.UpdatingManifests
                );
                
                manifestsChanged = await UpdateDeploymentManifests(gitRepository, 
                    deploymentUpdate, 
                    imageToFilenameMap, 
                    imageToTagInManifest.TryGetAndReturn(deploymentUpdate.Image) ?? "n/a", 
                    applicationSourcePath, 
                    yamlUtilities, 
                    relativePath);

                if (manifestsChanged)
                {
                    imageToTagInManifest[deploymentUpdate.Image] = deploymentUpdate.TargetTag;
                }
                
                await _deploymentService.FinishDeploymentUpdate(
                    deploymentUpdate,
                    manifestsChanged ? DeploymentUpdateStatus.Complete : DeploymentUpdateStatus.Failed
                );
            }
            
            if (!manifestsChanged)
            {
                // we don't have a deployment, ensure application manifest is up to date with latest image tags
                foreach (var keyValuePair in imageToTagInManifest)
                {
                    _applicationService.SetCurrentImageTag(application, keyValuePair.Key, keyValuePair.Value);   
                }
            }
            
            return manifestsChanged;
        }

        private async Task<bool> UpdateDeploymentManifests(
            Repository gitRepository, 
            DeploymentUpdate deploymentUpdate,
            Dictionary<Image, string> imageToFilenameMap, 
            string currentImageTag,
            string applicationSourcePath,
            YamlUtilities yamlUtilities, 
            string relativePath)
        {
            if (!imageToFilenameMap.TryGetValue(deploymentUpdate.Image, out string file))
            {
                // TODO: warn that we have an image tag update but no corresponding file
                _log.LogWarning("Update to {Repository} cannot be applied since there isn't matching file.");
                return false;
            }

            _log.LogInformation("Upgrading {Repository} to {NewTag} from {Tag}",
                deploymentUpdate.Image.Repository,
                deploymentUpdate.TargetTag,
                currentImageTag
            );

            var yaml = new YamlStream();
            var filePath = Path.Combine(applicationSourcePath, file);

            yamlUtilities.ReadYamlStream(yaml, filePath);

            var image = deploymentUpdate.Image;

            var doCommit = false;

            foreach (var doc in yaml.Documents)
            {
                var tagInManifest = yamlUtilities.ExtractValueFromDoc(image.TagProperty.Path, doc);
                if (tagInManifest == null) continue;

                if (tagInManifest == deploymentUpdate.TargetTag)
                {
                    _log.LogInformation("Tag for {Repository} matches new tag {NewTag}", image.Repository, deploymentUpdate.TargetTag);
                    continue;
                }

                _log.LogInformation("Setting current-tag for {Repository} to {Tag}", image.Repository,
                    deploymentUpdate.TargetTag);
                yamlUtilities.SetValueInDoc(image.TagProperty.Path, doc, deploymentUpdate.TargetTag);

                doCommit = true;
            }

            if (!doCommit)
                return true;

            yamlUtilities.WriteYamlStream(yaml, filePath);

            _log.LogInformation("Adding {Path} to repository staging", filePath);
            var gitFilePath = Path.Combine(relativePath, file);
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
