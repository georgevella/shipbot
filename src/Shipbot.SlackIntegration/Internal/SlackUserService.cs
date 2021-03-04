namespace Shipbot.SlackIntegration.Internal
{
    public class SlackUserService : ISlackUserService
    {
        private readonly SlackClientWrapper _slackClientWrapper;

        public SlackUserService(SlackClientWrapper slackClientWrapper)
        {
            _slackClientWrapper = slackClientWrapper;
        }
    }
}