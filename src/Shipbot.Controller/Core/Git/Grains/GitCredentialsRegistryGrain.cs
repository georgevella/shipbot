using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Shipbot.Controller.Core.Git.Exceptions;
using Shipbot.Controller.Core.Git.Models;

namespace Shipbot.Controller.Core.Git.Grains
{
    public class GitCredentialsRegistryGrain : Grain<Dictionary<string, GitCredentials>>, IGitCredentialsRegistryGrain
    {
        public Task<GitCredentials> GetCredentialByName(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (name.Trim().Length == 0) throw new ArgumentOutOfRangeException(nameof(name));
            
            if (State.ContainsKey(name))
            {
                return Task.FromResult(State[name]);
            }

            throw new GitCredentialRegistryException($"GitCredentials for '{name}' not found.");
        }

        public Task AddCredentials(string name, GitCredentials credentials)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (name.Trim().Length == 0) throw new ArgumentOutOfRangeException(nameof(name));

            if (State.ContainsKey(name))
                throw new Exception("Duplicate name exception");

            State[name] = credentials ?? throw new ArgumentNullException(nameof(credentials));

            return Task.CompletedTask;
        }

        public Task<bool> Contains(string credentialsKey)
        {
            return Task.FromResult(State.ContainsKey(credentialsKey));
        }
    }
}