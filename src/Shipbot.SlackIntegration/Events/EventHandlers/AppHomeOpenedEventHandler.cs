using System.Threading.Tasks;
using Shipbot.SlackIntegration.Internal;
using Slack.NetStandard.EventsApi.CallbackEvents;

namespace Shipbot.SlackIntegration.Events.EventHandlers
{
    public class AppHomeOpenedEventHandler : BaseSlackEventHandler<AppHomeOpened>
    {
        private readonly IAppHomeManager _appHomeManager;

        public AppHomeOpenedEventHandler(IAppHomeManager appHomeManager)
        {
            _appHomeManager = appHomeManager;
        }

        public override Task Process(AppHomeOpened callbackEvent)
        {
            return _appHomeManager.Publish(callbackEvent.User);
        }
    }
}