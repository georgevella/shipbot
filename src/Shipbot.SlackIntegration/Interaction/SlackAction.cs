using System;
using Slack.NetStandard.Interaction;

namespace Shipbot.SlackIntegration.Interaction
{
    public class SlackAction
    {
        public SlackAction(PayloadAction action, BlockActionsPayload payload)
        {
            Action = action;
            Payload = payload;
        }

        public string ActionId => Action.ActionId;

        public PayloadAction Action { get; }
        
        public BlockActionsPayload Payload { get; }
        public string Value => Action.Value;
    }
}