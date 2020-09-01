using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Shipbot.Tests.Utils
{
    public abstract class BaseUnitTestClass
    {
        private readonly LoggerFactory _loggerFactory;

        protected BaseUnitTestClass(ITestOutputHelper testOutputHelper)
        {
            _loggerFactory = new LoggerFactory();
            _loggerFactory.AddProvider(new XunitLoggerProvider(testOutputHelper));
        }

        public ILogger<T> GetLogger<T>()
        {
            return _loggerFactory.CreateLogger<T>();
        }
    }
}