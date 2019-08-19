using Shipbot.Controller.Core.Configuration.Git;

namespace Shipbot.Controller.Core.ApplicationSources
{
    public abstract class GitCredentials
    {
        
    }

    public class UsernamePasswordGitCredentials : GitCredentials
    {
        public UsernamePasswordGitCredentials(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string Username { get; }
        
        public string Password { get; }
    }
    
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

    public class SshGitCredentials : GitCredentials
    {
        public string SshPrivateKey { get; }

        public SshGitCredentials(string sshPrivateKey)
        {
            SshPrivateKey = sshPrivateKey;
        }
    }
}