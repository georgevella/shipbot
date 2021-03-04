using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shipbot.SlackIntegration.Events;
using Slack.NetStandard.EventsApi.CallbackEvents;

namespace Shipbot.SlackIntegration.Internal
{
    internal class SlackEventDispatcher :
        BaseSlackDispatcher<ICallbackEvent, ISlackEventHandler>, 
        ISlackEventDispatcher
    {
        // private readonly IServiceProvider _serviceProvider;
        // private readonly Dictionary<Type, Type> _handlerMap;

        // public SlackEventDispatcher(IServiceProvider serviceProvider, IEnumerable<ISlackEventHandler> handlers)
        // {
        //     _serviceProvider = serviceProvider;
        //     _handlerMap = handlers
        //         .Select(
        //             handler =>
        //             {
        //                 var handlerType = handler.GetType();
        //                 var typedHandlerInterface = handlerType.GetInterfaces().FirstOrDefault(
        //                     interfaceType => interfaceType.GetGenericTypeDefinition() == typeof(ISlackEventHandler<>)
        //                 );
        //                 var eventType = typedHandlerInterface?.GenericTypeArguments[0];
        //                 
        //                 return new
        //                 {
        //                     eventType,
        //                     handlerType
        //                 };
        //             }
        //         )
        //         .Where(x => x.eventType != null)
        //         .ToDictionary(x => x.eventType!, x => x.handlerType);
        // }

        // public async Task<object> DispatchCallbackEvent(ICallbackEvent callbackEvent)
        // {
        //     var eventType = callbackEvent.GetType();
        //
        //     if (!_handlerMap.TryGetValue(eventType, out var handlerType))
        //         return new object();
        //
        //     using var scope = _serviceProvider.CreateScope();
        //     var handler = (ISlackEventHandler) scope.ServiceProvider.GetService(handlerType);
        //     await handler.Invoke(callbackEvent);
        //     
        //     return new object();
        // }

        protected override string GetKeyForHandler(Type handlerType)
        {
            var typedHandlerInterface = handlerType.GetInterfaces().FirstOrDefault(
                interfaceType => interfaceType.GetGenericTypeDefinition() == typeof(ISlackEventHandler<>)
            );
            var eventType = typedHandlerInterface?.GenericTypeArguments[0];
            return eventType?.FullName ?? throw new InvalidOperationException();
        }

        protected override string GetKeyFromDispatchPayload(ICallbackEvent payload)
        {
            return payload.GetType().FullName;
        }

        public SlackEventDispatcher(IEnumerable<ISlackEventHandler> handlers) : base(handlers)
        {
        }
    }
}