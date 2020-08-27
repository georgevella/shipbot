using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shipbot.Controller.Core.Configuration;
using Shipbot.Data;
using Shipbot.SlackIntegration;
using Shipbot.SlackIntegration.Dao;
using Shipbot.SlackIntegration.Internal;
using Xunit;
using Xunit.Abstractions;

namespace Shipbot.Tests
{
    public class SlackMessageBuilderTests
    {
        private readonly LoggerFactory _loggerFactory;

        public SlackMessageBuilderTests(ITestOutputHelper testOutputHelper)
        {
            _loggerFactory = new LoggerFactory();
            _loggerFactory.AddProvider(new XunitLoggerProvider(testOutputHelper));
        }
        
        [Fact]
        public async Task X()
        {
            // setup
            var builder = new DbContextOptionsBuilder<ShipbotDbContext>()
                .UseInMemoryDatabase("SlackMessageTest");
            var options = builder.Options;

            var context = new ShipbotDbContext(
                options, 
                new[] {new SlackIntegrationDbContextConfigurator()}
                );
            var repo = new EntityRepository<SlackMessage>(context, new UnitOfWork(context));
            
            var log = _loggerFactory.CreateLogger<SlackClient>();
            var slackConfiguration = new OptionsWrapper<SlackConfiguration>(new SlackConfiguration()
            {
                Token = "xoxb-720397510838-721891676752-0ZZsUFHop90k4lOGdHkmn1qx",
                Timeout = 0
            });
            var client = new SlackClient(
                log,
                slackConfiguration, 
                repo,
                new SlackClientWrapper(slackConfiguration)
                );

            // run
            var handle = await client.PostMessageAsync("slack-bots-and-more", new SlackMessageBuilder("hello world").Build());

            await client.UpdateMessageAsync(handle, new SlackMessageBuilder("goodbye!").Build());
            
            // verify
        }
    }
    
    public class XunitLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public XunitLoggerProvider(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public ILogger CreateLogger(string categoryName)
            => new XunitLogger(_testOutputHelper, categoryName);

        public void Dispose()
        { }
    }

    public class XunitLogger : ILogger
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly string _categoryName;

        public XunitLogger(ITestOutputHelper testOutputHelper, string categoryName)
        {
            _testOutputHelper = testOutputHelper;
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state)
            => NoopDisposable.Instance;

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _testOutputHelper.WriteLine($"{_categoryName} [{eventId}] {formatter(state, exception)}");
            if (exception != null)
                _testOutputHelper.WriteLine(exception.ToString());
        }

        private class NoopDisposable : IDisposable
        {
            public static NoopDisposable Instance = new NoopDisposable();
            public void Dispose()
            { }
        }
    }
}