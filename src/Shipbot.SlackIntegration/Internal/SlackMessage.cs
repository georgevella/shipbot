using SlackAPI;

namespace Shipbot.SlackIntegration.Internal
{
    internal class SlackMessage : IMessage
    {
        public SlackMessage(string message) : this(message, new IBlock[] { })
        {
            
        }
        
        public SlackMessage(string message, IBlock[] blocks)
        {
            Message = message;
            Blocks = blocks;
        }

        public string Message { get; }
            
        public IBlock[] Blocks { get; }
    }
}