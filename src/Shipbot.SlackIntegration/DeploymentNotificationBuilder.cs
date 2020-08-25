using Shipbot.Models;
using Shipbot.SlackIntegration.Internal;
using SlackAPI;

namespace Shipbot.SlackIntegration
{
    public class DeploymentNotificationBuilder : IDeploymentNotificationBuilder
    {
        public IMessage BuildNotification(DeploymentUpdate deploymentUpdate, DeploymentUpdateStatus status)
        {
            return new SlackMessage(
                $"A new image of *{deploymentUpdate.Image.Repository}* was detected (tag *{deploymentUpdate.TargetTag}*).",
                new IBlock[]
                {
                    new SectionBlock()
                    {
                        text = new Text()
                        {
                            type = "mrkdwn",
                            text =
                                $"A new image of *{deploymentUpdate.Image.Repository}* was detected (tag *{deploymentUpdate.TargetTag}*)."
                        }
                    },
                    new DividerBlock(),
                    new SectionBlock()
                    {
                        fields = new Text[]
                        {
                            new Text()
                            {
                                text = $"*From*\n{deploymentUpdate.CurrentTag}",
                                type = "mrkdwn"
                            },
                            new Text()
                            {
                                text = $"*To*\n{deploymentUpdate.TargetTag}",
                                type = "mrkdwn"
                            },
                        }
                    },
                    new SectionBlock()
                    {
                        fields = new Text[]
                        {
                            new Text()
                            {
                                text = $"*Application*\n{deploymentUpdate.Application}",
                                type = "mrkdwn"
                            },
                            new Text()
                            {
                                text = $"*Status*\n{status}",
                                type = "mrkdwn"
                            },
                        }
                    }
                }
            );
        }
    }

    public interface IDeploymentNotificationBuilder
    {
        IMessage BuildNotification(DeploymentUpdate deploymentUpdate, DeploymentUpdateStatus status);
    }
}