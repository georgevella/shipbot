using System.Threading.Tasks;
using Orleans;
using Shipbot.Controller.Core.Configuration.Apps;

namespace Shipbot.Controller.Core.Apps.Grains
{
    public interface IApplicationGrain : IGrainWithStringKey
    {
        Task Configure(ApplicationDefinition applicationDefinition);
    }
}