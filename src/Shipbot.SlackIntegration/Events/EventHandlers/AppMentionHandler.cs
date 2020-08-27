using System.Threading.Tasks;
using Slack.NetStandard.EventsApi.CallbackEvents;

namespace Shipbot.SlackIntegration.Events.EventHandlers
{
    public class AppMentionHandler : BaseSlackEventHandler<AppMention>
    {
        private readonly ISlackClient _slackClient;

        public AppMentionHandler(ISlackClient slackClient)
        {
            _slackClient = slackClient;
        }

        protected override async Task Invoke(AppMention callbackEvent)
        {
            if (callbackEvent.Text.Contains("hello"))
            {
                await _slackClient.SendMessage(callbackEvent.Channel, "hello back");
            }
        }
    }
}