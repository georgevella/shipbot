using SlackAPI;

namespace Shipbot.Controller.Core.Slack
{
    public class SlackMessage
    {
        public string Message { get; }
            
        public IBlock[] Blocks { get; }

        public SlackMessage(string message, IBlock[] blocks = null)
        {
            Message = message;
            Blocks = blocks;
        }
    }
}