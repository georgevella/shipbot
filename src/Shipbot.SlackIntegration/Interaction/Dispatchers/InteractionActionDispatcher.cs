using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Shipbot.SlackIntegration.Internal;
using Slack.NetStandard.Interaction;

namespace Shipbot.SlackIntegration.Interaction.Dispatchers
{
    internal class InteractionActionDispatcher: 
        BaseSlackDispatcher<SlackAction, ISlackActionHandler>,
        ISlackInteractionActionDispatcher
    {
        public InteractionActionDispatcher(IEnumerable<ISlackActionHandler> handlers) : base(handlers)
        {
        }
        protected override Task HandleUnprocessed(SlackAction payload)
        {
            return Task.FromResult(new object());
        }
        protected override string GetKeyForHandler(Type handlerType)
        {
            var attribute = handlerType.GetCustomAttribute<SlackActionAttribute>();
            return attribute.ActionId;
        }
        protected override string GetKeyFromDispatchPayload(SlackAction payload) => payload.ActionId;
    }
}