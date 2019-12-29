using System;

namespace Shipbot.Controller.Core.Git.Exceptions
{
    public class GitCredentialRegistryException : Exception
    {
        public GitCredentialRegistryException(string message) : base(message)
        {            
        }
    }
}