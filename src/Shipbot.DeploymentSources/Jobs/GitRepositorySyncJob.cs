using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Util;
using Shipbot.Applications;
using Shipbot.Applications.Models;
using Shipbot.Controller.Core.ApplicationSources.Models;
using Shipbot.Controller.Core.Configuration;
using Shipbot.Deployments;
using Shipbot.Deployments.Models;
using Shipbot.JobScheduling;
using Shipbot.Models;
using YamlDotNet.RepresentationModel;

namespace Shipbot.Controller.Core.ApplicationSources.Jobs
{
    [DisallowConcurrentExecution]
    public partial class GitRepositorySyncJob : BaseJobWithData<DeploymentManifestSourceTrackingContext>
    {
        private readonly ILogger<GitRepositorySyncJob> _log;
        private readonly IApplicationService _applicationService;
        private readonly IDeploymentService _deploymentService;
        private readonly IDeploymentQueueService _deploymentQueueService;
        private readonly IOptions<ShipbotConfiguration> _configuration;
        private readonly IApplicationImageInstanceService _applicationImageInstanceService;

        public GitRepositorySyncJob(
            ILogger<GitRepositorySyncJob> log,
            IApplicationService applicationService,
            IDeploymentService deploymentService,
            IDeploymentQueueService deploymentQueueService,
            IOptions<ShipbotConfiguration> configuration,
            IApplicationImageInstanceService applicationImageInstanceService
        )
        {
            _log = log;
            _applicationService = applicationService;
            _deploymentService = deploymentService;
            _deploymentQueueService = deploymentQueueService;
            _configuration = configuration;
            _applicationImageInstanceService = applicationImageInstanceService;
        }

        public override async Task Execute(DeploymentManifestSourceTrackingContext context)
        {
            using (_log.BeginScope(new Dictionary<string, object>
            {
                {"Application", context.ApplicationName},
                {"Ref", context.DeploymentManifest.Repository.Ref},
                {"Path", context.DeploymentManifest.Path}
            }))
            {
                var repository = context.DeploymentManifest.Repository;

                // TODO: improve this to not have passwords in memory / use SecureStrings
                var credentials = (UsernamePasswordGitCredentials) repository.Credentials;

                using var gitRepository = new Repository(context.GitRepositoryPath);

                var branch = CheckoutDeploymentManifest(gitRepository, repository, credentials);
                
                // TODO: handle scenario when we are tracking a git commit or a tag
                var changesDone = context.DeploymentManifest switch
                {
                    HelmDeploymentManifest helmDeploymentManifest => await SynchronizeHelmApplicationSource(
                        gitRepository, context, helmDeploymentManifest),
                    RawDeploymentManifest rawDeploymentManifest => await SynchronizeRawDeploymentManifestSource(
                        gitRepository, context, rawDeploymentManifest),
                    _ => false
                };
                
                if (
                    changesDone &&
                    context.AutoDeploy &&
                    !_configuration.Value.Dryrun)
                {
                    var attempt = 3;
                    var successful = false;
                    while (attempt > 0)
                    {
                        _log.LogInformation("Pushing repository changes for {Application}", context.ApplicationName);

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

                            successful = true;
                            break;
                        }
                        catch (NonFastForwardException e)
                        {
                            HandleGitFastForwardException(gitRepository);
                            --attempt;
                        }
                    }
                    
                    if (!successful)
                    {
                        throw new InvalidOperationException(
                            $"Failed to push latest commits for {repository.Uri}/{repository.Ref}");
                    }

                }
            }
        }

        private void HandleGitFastForwardException(Repository gitRepository)
        {
            _log.LogInformation("Remote has newer commits, re-fetching");
            var remote = gitRepository.Network.Remotes["origin"];
            var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
            Commands.Fetch(gitRepository, remote.Name, refSpecs, null, "");

            var rebaseIdentity = new Identity("deploy-bot", "deploy-bot@riverigaming.com");
            var rebaseOptions = new RebaseOptions()
            {
                FileConflictStrategy = CheckoutFileConflictStrategy.Theirs
            };

            _log.LogInformation("Rebasing on remote branch prior to trying to re-push changes ...");
            var upstream = gitRepository.Head.TrackedBranch;
            var rebaseResult = gitRepository.Rebase.Start(
                gitRepository.Head,
                upstream,
                upstream,
                rebaseIdentity,
                rebaseOptions
            );

            // TODO: handle rebaseResult nulliness
            while (rebaseResult.Status != RebaseStatus.Complete)
            {
                // we should hit here only if we find a conflict, since we don't use interactive mode on rebase
                var step = rebaseResult.CurrentStepInfo ?? gitRepository.Rebase.GetCurrentStepInfo();

                var currentStatus = gitRepository.RetrieveStatus();
                foreach (var item in currentStatus)
                {
                    Commands.Stage(gitRepository, item.FilePath);
                }

                rebaseResult = gitRepository.Rebase.Continue(rebaseIdentity, rebaseOptions);
            }
        }

        private Branch CheckoutDeploymentManifest(Repository gitRepository, DeploymentManifestSource repository,
            UsernamePasswordGitCredentials credentials)
        {
            _log.LogTrace("CheckoutDeploymentManifest ...");
            
            var branchNames = gitRepository.Branches.Select(x => x.CanonicalName).ToList();

            var branch = gitRepository.Branches.FirstOrDefault(b => b.FriendlyName == repository.Ref);
            var originBranch =
                gitRepository.Branches.FirstOrDefault(b => b.FriendlyName == $"origin/{repository.Ref}");
            if (branch != null || originBranch != null)
            {
                if (branch == null)
                {
                    // checkout branch from origin and make sure we are tracking the remote branch
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

                _log.LogTrace("Fetching latest sources for {branch} [{currentHash}] ...",
                    branch.CanonicalName, currentHash);

                // TODO: there is a problem here - if the remote is updated and we have local changes, we need to determine
                // an automated way to either:
                // 1. rebase on the remote
                // 2. if there are conflicts, ignore all changes, clone again and reapply the changes.
                var attempt = 3;
                var successful = false;

                while (attempt > 0)
                {
                    try
                    {
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
                                    },
                                },
                                MergeOptions = new MergeOptions()
                                {
                                    FastForwardStrategy = FastForwardStrategy.FastForwardOnly,
                                },
                            });

                        successful = true;
                        break;
                    }
                    catch (NonFastForwardException e)
                    {
                        HandleGitFastForwardException(gitRepository);
                        --attempt;
                    }
                }

                if (!successful)
                {
                    throw new InvalidOperationException(
                        $"Failed to retrieve latest commits for {repository.Uri}/{repository.Ref}");
                }

                if (gitRepository.Head.Tip.Sha != currentHash)
                {
                    _log.LogTrace("Branch changed, triggered application refresh");
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

        class DeploymentManifestSourceSyncContext
        {
            public DeploymentManifestSourceSyncContext(
                IReadOnlyDictionary<ApplicationImage, FileInfo> applicationImageToFileMapping, 
                IReadOnlyDictionary<ApplicationImage, string> manifestTags
                )
            {
                ApplicationImageToFileMapping = applicationImageToFileMapping;
                ManifestTags = manifestTags;
            }

            /// <summary>
            ///     Contains a map between every application image and the file that stores it's tag
            /// </summary>
            public IReadOnlyDictionary<ApplicationImage, FileInfo> ApplicationImageToFileMapping { get; }
            
            /// <summary>
            ///     Contains the application image tags defined in the manifest. 
            /// </summary>
            public IReadOnlyDictionary<ApplicationImage, string> ManifestTags { get; }
            
            public static DeploymentManifestSourceSyncContext Build(
                Application application,
                IEnumerable<FileInfo> files
            )
            {
                // _log.LogTrace("Beginning to parse value files defined in application source ...");
                
                var yamlUtilities = new YamlUtilities();
                var imageToFilenameMap = new Dictionary<ApplicationImage, FileInfo>();
                var imageToTagInManifest = new Dictionary<ApplicationImage, string>();
                
                foreach (var file in files)
                {
                    var yaml = new YamlStream();
                    //var filePath = Path.Combine(applicationSourcePath, file);
                    yamlUtilities.ReadYamlStream(yaml, file.FullName);

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

                // _log.LogTrace("Completing parsing value files defined in application source ...");
                return new DeploymentManifestSourceSyncContext(imageToFilenameMap, imageToTagInManifest);
            }
            
        }

        
    }
}
