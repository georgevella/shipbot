using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shipbot.SlackIntegration.Dto.Slack.ExternalOptions;
using Shipbot.SlackIntegration.ExternalOptions;
using Slack.NetStandard;
using Slack.NetStandard.Messages.Elements;

namespace Shipbot.Applications.Slack
{
    [SlackExternalOptions("app-name-selection")]
    public class ApplicationIdsExternalOptionsSource : ISlackExternalOptionsSource
    {
        private readonly IApplicationService _applicationService;

        public ApplicationIdsExternalOptionsSource(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }
        
        public Task<IEnumerable<IOption>> Process(BlockSuggestionPayload blockSuggestionPayload)
        {
            return Task.FromResult(
                _applicationService.GetApplications()
                    .Select(
                        x => (IOption) new Option()
                        {
                            Text = new PlainText(x.Name),
                            Value = x.Name
                        })
            );
        }
    }
}