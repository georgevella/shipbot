using System.Threading.Tasks;

namespace Shipbot.SlackIntegration.Commands
{
    public interface ISlackCommandHandler
    {
        Task Invoke(string channel, string[] args);
    }
}