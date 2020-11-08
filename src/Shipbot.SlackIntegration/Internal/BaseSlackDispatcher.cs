using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shipbot.SlackIntegration.Internal
{ 
    // where TDispatchHandler : ISlackPayloadProcessor<TDispatchResult, TDispatchPayload>
    internal abstract class BaseSlackDispatcherWorker<TDispatchPayload, TDispatchHandler>
    {
        private readonly Dictionary<string, TDispatchHandler> _handlerMap;

        public BaseSlackDispatcherWorker(IEnumerable<TDispatchHandler> handlers)
        {
            _handlerMap = handlers
                .Select(
                    handler =>
                    {
                        var handlerType = handler.GetType();
                        var key = GetKeyForHandler(handlerType);
                        return new
                        {
                            key,
                            handler
                        };
                    }
                )
                .ToDictionary(x => x.key, x => x.handler);
        }

        protected bool GetHandler(string key, out TDispatchHandler handler)
        {
            return _handlerMap.TryGetValue(key, out handler);
        }

        protected abstract string GetKeyForHandler(Type handlerType);
        protected abstract string GetKeyFromDispatchPayload(TDispatchPayload payload);
    }

    abstract class BaseSlackDispatcher<TDispatchPayload, TDispatchResult, TDispatchHandler> 
        : BaseSlackDispatcherWorker<TDispatchPayload, TDispatchHandler>, ISlackPayloadDispatcher<TDispatchPayload, TDispatchResult>
        where TDispatchHandler : ISlackPayloadProcessor<TDispatchPayload, TDispatchResult>
    {
        public async Task<TDispatchResult> Dispatch(TDispatchPayload payload)
        {
            var key = GetKeyFromDispatchPayload(payload);

            if (GetHandler(key, out var handler))
            {
                await handler.Process(payload);
            }

            return await HandleUnprocessed(payload);
        }
        
        protected abstract Task<TDispatchResult> HandleUnprocessed(TDispatchPayload payload);

        protected BaseSlackDispatcher(IEnumerable<TDispatchHandler> handlers) : base(handlers)
        {
        }
    }
    
    abstract class BaseSlackDispatcher<TDispatchPayload, TDispatchHandler> 
        : BaseSlackDispatcherWorker<TDispatchPayload, TDispatchHandler>, ISlackPayloadDispatcher<TDispatchPayload>
        where TDispatchHandler : ISlackPayloadProcessor<TDispatchPayload>
    {
        public async Task Dispatch(TDispatchPayload payload)
        {
            var key = GetKeyFromDispatchPayload(payload);

            if (GetHandler(key, out var handler))
            {
                await handler.Process(payload);
            }

            await HandleUnprocessed(payload);
        }

        protected virtual Task HandleUnprocessed(TDispatchPayload payload)
        {
            return Task.CompletedTask;
        }

        protected BaseSlackDispatcher(IEnumerable<TDispatchHandler> handlers) : base(handlers)
        {
        }
    }
}