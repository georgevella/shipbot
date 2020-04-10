using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Orleans.Streams;

namespace Shipbot.Controller.Core.Utilities.Eventing
{
    public interface IHandleEvent<T> : IGrain
    {
        
    }

    internal class EventHandlingConstants
    {
        public const string EventHandlingStreamProvider = "EventHandlingStream";
        public const string EventHandlingNamespace = "EventHandling";
    }

    public abstract class EventHandlingGrain<TState> : Grain<TState>
    {
        protected async Task SubscribeForEvents<TEvent>(Func<TEvent, StreamSequenceToken, Task> handler)
        {
            var stream = GetStream<TEvent>();
            var handles = await stream.GetAllSubscriptionHandles();

            if (handles.Any())
            {
                await ResumeSubscriptions(handles, handler);
            }
            else
            {
                await stream.SubscribeAsync(handler);
            }
        }

        private IAsyncStream<TEvent> GetStream<TEvent>()
        {
            var streamProvider = GetStreamProvider(EventHandlingConstants.EventHandlingStreamProvider);

            var stream = streamProvider.GetStream<TEvent>(
                typeof(TEvent).FullName!.CreateGuidFromString(),
                EventHandlingConstants.EventHandlingNamespace);
            return stream;
        }

        protected async Task SendEvent<TEvent>(TEvent e)
        {
            var stream = GetStream<TEvent>();
            await stream.OnNextAsync(e);
        } 

        private async Task ResumeSubscriptions<TEvent>(IList<StreamSubscriptionHandle<TEvent>> handles,
            Func<TEvent, StreamSequenceToken, Task> handler)
        {
            foreach (var handle in handles)
            {
                await handle.ResumeAsync(handler);
            }
        }
    } 
}