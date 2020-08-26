using Shipbot.Deployments.Models;
using Shipbot.Models;
using Shipbot.SlackIntegration;

namespace Shipbot.Deployments
{
    public class DeploymentNotificationBuilder : IDeploymentNotificationBuilder
    {
        public IMessage BuildNotification(Deployment deployment)
        {
            var builder = new SlackMessageBuilder($"A new image of *{deployment.ImageRepository}* was detected (tag *{deployment.TargetTag}*).");
            return builder.AddSection(
                $"A new image of *{deployment.ImageRepository}* was detected (tag *{deployment.TargetTag}*)."
                )
                .AddDivider()
                .AddSection(fields: new []
                {
                    $"*From*\n{deployment.CurrentTag}",
                    $"*To*\n{deployment.TargetTag}",
                    $"*Application*\n{deployment.ApplicationId}",
                    $"*Status*\n{deployment.Status}"
                }
                ).Build();
        }
    }

    public interface IDeploymentNotificationBuilder
    {
        IMessage BuildNotification(Deployment deployment);
    }
}