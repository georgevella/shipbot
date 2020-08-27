using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Slack.NetStandard.EventsApi.CallbackEvents;

namespace Shipbot.SlackIntegration.Events
{
    internal class SlackEventDispatcher : ISlackEventDispatcher
    {
        private readonly Dictionary<Type, ISlackEventHandler> _handlerMap;

        public SlackEventDispatcher(IEnumerable<ISlackEventHandler> handlers)
        {
            _handlerMap = handlers
                .Select(
                    handler =>
                    {
                        var handlerType = handler.GetType();
                        var typedHandlerInterface = handlerType.GetInterfaces().FirstOrDefault(
                            interfaceType => interfaceType.GetGenericTypeDefinition() == typeof(ISlackEventHandler<>)
                        );
                        var eventType = typedHandlerInterface?.GenericTypeArguments[0];

                        return new
                        {
                            eventType,
                            handler
                        };
                    }
                )
                .Where(x => x.eventType != null)
                .ToDictionary(x => x.eventType, x => x.handler);
        }

        public async Task<object> DispatchCallbackEvent(ICallbackEvent callbackEvent)
        {
            var eventType = callbackEvent.GetType();
            var handler = _handlerMap[eventType];

            await handler.Invoke(callbackEvent);
            
            return new object();
        }
    }

    public interface ISlackEventDispatcher
    {
        Task<object> DispatchCallbackEvent(ICallbackEvent callbackEvent);
    }
}