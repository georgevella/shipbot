using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Shipbot.SlackIntegration.Internal;
using Slack.NetStandard.Interaction;

namespace Shipbot.SlackIntegration.Interaction
{
    internal class ShortcutInteractionDispatcher : 
        BaseSlackDispatcher<ShortcutPayload, ISlackGlobalShortcutHandler>,
        ISlackShortcutInteractionDispatcher
    {
        private static readonly object DefaultResult = new object();
        
        protected override Task HandleUnprocessed(ShortcutPayload payload)
        {
            return Task.FromResult(DefaultResult);
        }
        protected override string GetKeyForHandler(Type handlerType)
        {
            var slackInteractionAttribute = handlerType.GetCustomAttribute<SlackInteractionAttribute>();
            return slackInteractionAttribute.CallbackId;
        }
        protected override string GetKeyFromDispatchPayload(ShortcutPayload payload)
        {
            return payload.CallbackId;
        }

        public ShortcutInteractionDispatcher(IEnumerable<ISlackGlobalShortcutHandler> handlers) : base(handlers)
        {
        }
    }
}