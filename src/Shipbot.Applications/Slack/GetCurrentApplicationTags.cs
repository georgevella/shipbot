using System.Linq;
using System.Threading.Tasks;
using Shipbot.Applications.Models;
using Shipbot.Models;
using Shipbot.SlackIntegration;
using Shipbot.SlackIntegration.Commands;

namespace Shipbot.Applications.Slack
{
    [SlackCommand("get-current-tags", group: "apps")]
    public class GetCurrentApplicationTags : ISlackCommandHandler
    {
        private readonly IApplicationService _applicationService;
        private readonly ISlackClient _slackClient;

        public GetCurrentApplicationTags(
            IApplicationService applicationService,
            ISlackClient slackClient
            )
        {
            _applicationService = applicationService;
            _slackClient = slackClient;
        }
        
        public Task Invoke(string channel, string[] args)
        {
            var id = args[0];
            var application = _applicationService.GetApplication(id);
            var result = _applicationService.GetCurrentImageTags(application)
                .ToDictionary(
                    x => x.Key.TagProperty.Path, 
                    x => x.Key.TagProperty.ValueFormat == TagPropertyValueFormat.TagOnly ? $"{x.Key.Repository}:{x.Value}" : $"{x.Value}"
                );

            var builder = new SlackMessageBuilder($"The services for application '{id}' have the following tags:")
                .AddDivider();
            
            var allFields = result.Select(x => $"*{x.Key}*\n {x.Value}").ToList();
            var startIndex = 0;

            do
            {
                var fields = allFields.Skip(startIndex).Take(10).ToList();
                builder.AddSection(fields: fields);
                startIndex += fields.Count;
            } while (startIndex < allFields.Count);

            var message = builder.Build();
            return _slackClient.PostMessageAsync(channel, message);
        }
    }
}