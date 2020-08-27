using System;
using System.Threading.Tasks;
using Shipbot.Models;

namespace Shipbot.SlackIntegration
{
    public interface ISlackClient : IDisposable
    {
        // Task<IMessageHandle> SendDeploymentUpdateNotification(
        //     string channel, 
        //     DeploymentUpdate deploymentUpdate,
        //     DeploymentUpdateStatus status
        // );
        //
        // Task<IMessageHandle> UpdateDeploymentUpdateNotification(
        //     IMessageHandle handle, 
        //     DeploymentUpdate deploymentUpdate,
        //     DeploymentUpdateStatus status
        // );
        Task<IMessageHandle> PostMessageAsync(string channelId, IMessage message);
        Task<IMessageHandle> UpdateMessageAsync(IMessageHandle messageHandle, IMessage handle);
        Task<IMessageHandle> SendMessage(string channel, string message);
    }
}