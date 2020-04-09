using System;
using System.Threading.Tasks;
using Octokit;
using Shipbot.Controller.Core.Deployments;
using Shipbot.Controller.Core.Deployments.Models;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Slack
{
    public interface ISlackClient : IDisposable
    {
        Task Connect();
        Task<IMessageHandle> SendMessage(string channel, string message);

        Task<IMessageHandle> SendDeploymentUpdateNotification(
            string channel, 
            DeploymentKey deployment
        );

        Task<IMessageHandle> UpdateDeploymentUpdateNotification(
            IMessageHandle handle, 
            DeploymentKey deployment
        );
    }
}