using Shipbot.Controller.Core.ApplicationSources.Models;
using Shipbot.Controller.Core.Configuration.Git;
using Shipbot.Models;

namespace Shipbot.Controller.Core.ApplicationSources
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