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
            var messageConfiguration = builder.AddSection(
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
                );

            if (deployment.Status == DeploymentStatus.Pending)
            {
                messageConfiguration.AddActions(
                    blockBuilder => blockBuilder
                        .AddButton("deploy", "Deploy", $"{deployment.Id:D}", style: "primary")
                    );
            }

            return messageConfiguration.Build();
        }
    }

    public interface IDeploymentNotificationBuilder
    {
        IMessage BuildNotification(Deployment deployment);
    }
}