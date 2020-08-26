using Shipbot.Models;

namespace Shipbot.SlackIntegration
{
    public class DeploymentNotificationBuilder : IDeploymentNotificationBuilder
    {
        public IMessage BuildNotification(DeploymentUpdate deploymentUpdate, DeploymentUpdateStatus status)
        {
            var builder = new SlackMessageBuilder($"A new image of *{deploymentUpdate.Image.Repository}* was detected (tag *{deploymentUpdate.TargetTag}*).");
            return builder.AddSection(
                $"A new image of *{deploymentUpdate.Image.Repository}* was detected (tag *{deploymentUpdate.TargetTag}*)."
                )
                .AddDivider()
                .AddSection(fields: new []
                {
                    $"*From*\n{deploymentUpdate.CurrentTag}",
                    $"*To*\n{deploymentUpdate.TargetTag}",
                    $"*Application*\n{deploymentUpdate.Application}",
                    $"*Status*\n{status}"
                }
                ).Build();
        }
    }

    public interface IDeploymentNotificationBuilder
    {
        IMessage BuildNotification(DeploymentUpdate deploymentUpdate, DeploymentUpdateStatus status);
    }
}