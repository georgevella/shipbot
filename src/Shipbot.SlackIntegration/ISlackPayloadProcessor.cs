using System.Threading.Tasks;

namespace Shipbot.SlackIntegration
{
    public interface ISlackPayloadDispatcher<in TDispatchPayload>
    {
        Task Dispatch(TDispatchPayload payload);
    }
    public interface ISlackPayloadDispatcher<in TDispatchPayload, TDispatchResult>
    {
        Task<TDispatchResult> Dispatch(TDispatchPayload payload);
    }

    public interface ISlackPayloadProcessor<in TDispatchPayload>
    {
        Task Process(TDispatchPayload payload);
    }
    
    public interface ISlackPayloadProcessor<in TDispatchPayload, TDispatchResult>
    {
        Task<TDispatchResult> Process(TDispatchPayload payload);
    }
}