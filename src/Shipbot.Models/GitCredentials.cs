
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

    public class SshGitCredentials : GitCredentials
    {
        public string SshPrivateKey { get; }

        public SshGitCredentials(string sshPrivateKey)
        {
            SshPrivateKey = sshPrivateKey;
        }
    }
}