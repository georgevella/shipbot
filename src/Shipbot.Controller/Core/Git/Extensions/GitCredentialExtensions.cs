using Shipbot.Controller.Core.Configuration.Git;
using Shipbot.Controller.Core.Git.Models;

namespace Shipbot.Controller.Core.Git.Extensions
{
    public static class GitCredentialExtensions {
        public static GitCredentials ConvertToGitCredentials(this GitCredentialSettings gitCredentialSettings)
        {
            return gitCredentialSettings.Type switch {
                CredentialType.Ssh => (GitCredentials) new SshGitCredentials(gitCredentialSettings.Ssh.PrivateKey),
                CredentialType.UsernamePassword => new UsernamePasswordGitCredentials(
                    gitCredentialSettings.UsernamePassword.Username,
                    gitCredentialSettings.UsernamePassword.Password
                )
            };
        }
    }
}