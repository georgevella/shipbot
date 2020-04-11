using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Configuration.ApplicationSources;
using Shipbot.Controller.Core.Deployments.GrainKeys;
using Shipbot.Controller.Core.Deployments.Models;
using Shipbot.Controller.Core.DeploymentSources.Exceptions;
using Shipbot.Controller.Core.DeploymentSources.GrainState;
using Shipbot.Controller.Core.DeploymentSources.Models;
using Shipbot.Controller.Core.Git.Models;
using Shipbot.Controller.Core.Models;
using Shipbot.Controller.Core.Utilities;

namespace Shipbot.Controller.Core.DeploymentSources.Grains
{
    public abstract class  GitBasedDeploymentSourceGrain<TState> : Grain<TState>, IDeploymentSourceGrain, IRemindable
        where TState : ApplicationSource, new()
    {
        private readonly ILogger _log;
        private DirectoryInfo _checkoutDirectory;
        private ApplicationEnvironmentKey _key;

        protected GitBasedDeploymentSourceGrain(
            ILogger log
            )
        {
            _log = log;
        }

        protected DirectoryInfo CheckoutDirectory => _checkoutDirectory;

        public override Task OnActivateAsync()
        {
            var invalids = Path.GetInvalidFileNameChars();
            var name = string.Join(
                    "_",
                    this.GetPrimaryKeyString().Split(invalids, StringSplitOptions.RemoveEmptyEntries)
                )
                .TrimEnd('.');
            var checkoutDirectoryPath = Path.Combine(Path.GetTempPath(), $"{name}-{Guid.NewGuid():D}");
            _checkoutDirectory = new DirectoryInfo(checkoutDirectoryPath);
            
            var key = this.GetPrimaryKeyString();
            _key = ApplicationEnvironmentKey.Parse(key);

            return base.OnActivateAsync();
        }

        public async Task Activate()
        {
            State.IsActive = true;

            await base.WriteStateAsync();

            // setup a reminder to refresh the git repository every one minute
            await RegisterOrUpdateReminder("GitRepoRefreshReminder", TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }
        
        public async Task Checkout()
        {
            _log.Info("Cloning {Repository} into {Path}",
                State.Repository.Uri,
                CheckoutDirectory.FullName);
            
            if (CheckoutDirectory.Exists)
            {
                CheckoutDirectory.Delete(true);
                CheckoutDirectory.Create();
            }

            var credentialsRegistry = GrainFactory.GetGitCredentialsRegistryGrain();
            var credentials = await credentialsRegistry.GetCredentialByName(State.Repository.CredentialsKey);

            var options = new CloneOptions();
            if (credentials is UsernamePasswordGitCredentials usernamePasswordGitCredentials)
            {
                options.CredentialsProvider = (url, fromUrl, types) => new UsernamePasswordCredentials()
                {
                    Username = usernamePasswordGitCredentials.Username,
                    Password = usernamePasswordGitCredentials.Password
                };
            }

            // TODO support for non-branch checkouts
            options.Checkout = true;
            options.BranchName = State.Repository.Ref;

            Repository.Clone(
                State.Repository.Uri.ToString(),
                CheckoutDirectory.FullName,
                options
            );
        }

        public virtual async Task Configure(
            ApplicationSourceSettings applicationSourceSettings,
            ApplicationEnvironmentKey applicationEnvironmentKey
        )
        {
            if (applicationSourceSettings == null) throw new ArgumentNullException(nameof(applicationSourceSettings));

            var credentialsRegistry = GrainFactory.GetGitCredentialsRegistryGrain();
            if (!await credentialsRegistry.Contains(applicationSourceSettings.Repository.Credentials))
            {
                throw new DeploymentSourceException("Supplied credential key is unknown");
            }
            
            State.Repository.Uri = new Uri(applicationSourceSettings.Repository.Uri);
            State.Repository.Ref = applicationSourceSettings.Repository.Ref;
            State.Repository.CredentialsKey = applicationSourceSettings.Repository.Credentials;

            State.Path = applicationSourceSettings.Path;

            State.ApplicationEnvironment = applicationEnvironmentKey;
        }
        
        public Task<IReadOnlyDictionary<string, string>> GetImageTags()
        {
            return Task.FromResult(
                (IReadOnlyDictionary<string, string>)State.Metadata.ImageTags.ToDictionary(
                    x => x.ValuePath,
                    x => x.Value
                )
            );
        }

        public abstract Task<DeploymentSourceChangeResult> ApplyDeploymentAction(DeploymentSourceChange deploymentSourceChange);

        public abstract Task Refresh();
        
        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            using (_log.BeginShipbotLogScope(_key))
            {
                await Refresh();

                var environmentGrain = GrainFactory.GetEnvironment(State.ApplicationEnvironment);

                foreach (var keyValuePair in State.Metadata.ImageTags)
                {
                    var currentTag = await environmentGrain.GetImageTag(keyValuePair.ValuePath);
                    if (currentTag != keyValuePair.Value)
                    {
                        await environmentGrain.SetImageTag(keyValuePair.ValuePath, keyValuePair.Value);
                    }
                }   
            }
        }
    }
}