using SlackAPI;

namespace Shipbot.SlackIntegration
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