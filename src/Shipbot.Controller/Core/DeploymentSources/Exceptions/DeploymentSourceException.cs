using System;

namespace Shipbot.Controller.Core.DeploymentSources.Exceptions
{
    public class DeploymentSourceException : Exception
    {
        public DeploymentSourceException(string message) :  base(message)
        {
            
        }
    }
}