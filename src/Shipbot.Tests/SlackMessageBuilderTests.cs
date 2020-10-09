using System.Threading.Tasks;
using DotNet.Globbing;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shipbot.ContainerRegistry.Models;
using Shipbot.ContainerRegistry.Services;
using Shipbot.Controller.Core.Configuration;
using Shipbot.Data;
using Shipbot.Models;
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
                repo,
                new SlackClientWrapper(slackConfiguration),
                new SlackApiClientWrapper(slackConfiguration)
                );

            // run
            var handle = await client.PostMessageAsync("slack-bots-and-more", new SlackMessageBuilder("hello world").Build());

            await client.UpdateMessageAsync(handle, new SlackMessageBuilder("goodbye!").Build());
            
            // verify
        }
    }
}