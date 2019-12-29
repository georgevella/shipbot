using System.Threading.Tasks;
using Orleans;
using Shipbot.Controller.Core.Git.Models;

namespace Shipbot.Controller.Core.Git.Grains
{
    public interface IGitCredentialsRegistryGrain : IGrainWithGuidKey
    {
        Task<GitCredentials> GetCredentialByName(string name);
        Task AddCredentials(string name, GitCredentials credentials);
        Task<bool> Contains(string credentialsKey);
    }
}