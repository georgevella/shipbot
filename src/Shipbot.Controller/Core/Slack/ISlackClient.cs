using System;
using System.Threading.Tasks;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Slack
{
    public interface ISlackClient : IDisposable
    {
        void Connect();
        Task<IMessageHandle> SendMessage(string channel, string message);

        Task<IMessageHandle> SendDeploymentUpdateNotification(
            string channel, 
            DeploymentUpdate deploymentUpdate
        );

        Task<IMessageHandle> UpdateDeploymentUpdateNotification(
            IMessageHandle handle, 
            DeploymentUpdate deploymentUpdate
        );
    }
}