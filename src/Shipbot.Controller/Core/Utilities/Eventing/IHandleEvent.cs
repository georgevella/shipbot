using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Orleans;
using Orleans.Streams;

namespace Shipbot.Controller.Core.Utilities.Eventing
{
    public interface IHandleEvent<T> : IGrain
    {
    }

    public abstract class EventHandlingGrain<TState> : Grain<TState>
    {
        protected async Task SubscribeForEvents<TEvent>(Func<TEvent, StreamSequenceToken, Task> handler)
        {
            var stream = GetEventStream<TEvent>();
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

        protected async Task SubscribeToPrivateMessaging<TMessage>(Guid id, string ns, Func<TMessage, StreamSequenceToken, Task> handler)
        {
            var r = this.GrainReference;
            
            var stream = GetPrivateMessagingStream<TMessage>(id, ns);
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

        private IAsyncStream<TEvent> GetEventStream<TEvent>()
        {
            var streamProvider = GetStreamProvider(EventingStreamingConstants.EventHandlingStreamProvider);

            var stream = streamProvider.GetStream<TEvent>(
                typeof(TEvent).FullName!.CreateGuidFromString(),
                EventingStreamingConstants.EventHandlingNamespace);
            return stream;
        }

        private IAsyncStream<TEvent> GetPrivateMessagingStream<TEvent>(Guid receiverGuid, string ns)
        {
            var streamProvider = GetStreamProvider(EventingStreamingConstants.EventHandlingStreamProvider);

            var stream = streamProvider.GetStream<TEvent>(
                receiverGuid,
                ns
            );
            return stream;
        }
        
        protected Task SendMessage<TMessage>(Guid receiver, TMessage message, string ns = EventingStreamingConstants.EventHandlingNamespace)
        {
            var stream = GetPrivateMessagingStream<TMessage>(receiver, ns);
            return stream.OnNextAsync(message);
        }

        protected Task SendEvent<TEvent>(TEvent e)
        {
            var stream = GetEventStream<TEvent>();
            return stream.OnNextAsync(e);
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