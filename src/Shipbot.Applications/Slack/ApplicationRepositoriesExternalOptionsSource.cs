using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shipbot.SlackIntegration.Dto.Slack.ExternalOptions;
using Shipbot.SlackIntegration.ExternalOptions;
using Slack.NetStandard.Messages.Elements;

namespace Shipbot.Applications.Slack
{
    [SlackExternalOptions("repository-selection")]
    public class ApplicationRepositoriesExternalOptionsSource : ISlackExternalOptionsSource
    {
        private readonly IApplicationService _applicationService;

        public ApplicationRepositoriesExternalOptionsSource(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }
        public Task<IEnumerable<IOption>> Process(BlockSuggestionPayload blockSuggestionPayload)
        {
            return Task.FromResult(Enumerable.Empty<IOption>());
        }
    }
}