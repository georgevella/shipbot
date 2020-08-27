using System;

namespace Shipbot.Data
{
    public abstract class BaseLazyLoadedPoco
    {
        private static readonly Action<object, string> EmptyLazyLoader = (_, __) => { };
        
        // ReSharper disable once MemberCanBePrivate.Global
        protected Action<object, string> LazyLoader { get; }

        protected BaseLazyLoadedPoco(Action<object, string>? lazyLoader = null)
        {
            
            LazyLoader = lazyLoader ?? EmptyLazyLoader;
        }
    }
}