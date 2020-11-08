using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Shipbot.SlackIntegration.Dto.Slack.ExternalOptions;
using Shipbot.SlackIntegration.Internal;
using Slack.NetStandard.Messages.Elements;

namespace Shipbot.SlackIntegration.ExternalOptions
{
    internal class ExternalOptionsProvider : 
        BaseSlackDispatcher<BlockSuggestionPayload, IEnumerable<IOption>, ISlackExternalOptionsSource>,
        ISlackExternalOptionsProvider
    {
        protected override Task<IEnumerable<IOption>> HandleUnprocessed(BlockSuggestionPayload payload)
        {
            return Task.FromResult(Enumerable.Empty<IOption>());
        }

        protected override string GetKeyForHandler(Type handlerType)
        {
            var slackInteractionAttribute = handlerType.GetCustomAttribute<SlackExternalOptionsAttribute>();
            return slackInteractionAttribute.ActionId;
        }

        protected override string GetKeyFromDispatchPayload(BlockSuggestionPayload payload) => payload.ActionId;

        public ExternalOptionsProvider(IEnumerable<ISlackExternalOptionsSource> handlers) : base(handlers)
        {
        }
    }
}