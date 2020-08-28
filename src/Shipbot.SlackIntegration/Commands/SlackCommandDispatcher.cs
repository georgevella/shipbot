using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Shipbot.SlackIntegration.Events;

namespace Shipbot.SlackIntegration.Commands
{
    public class SlackCommandDispatcher : ISlackCommandDispatcher
    {
        private readonly Dictionary<string, Dictionary<string, ISlackCommandHandler>> _supportedCommands;

        public SlackCommandDispatcher(IEnumerable<ISlackCommandHandler> handlers)
        {
            var list = handlers
                .Select(
                    handler =>
                    {
                        var handlerType = handler.GetType();

                        var attribute = handlerType.GetCustomAttribute<SlackCommandAttribute>();
                        return new
                        {
                            attribute,
                            handler
                        };
                    }
                )
                .Where(x => x.attribute != null)
                .ToList();

            _supportedCommands = list
                .Select(handler =>
                {
                    if (string.IsNullOrEmpty(handler.attribute.Group))
                    {
                        return new
                        {
                            Command = handler.attribute.Name,
                            Subcommand = string.Empty,
                            Handler = handler.handler
                        };
                    }

                    return new
                    {
                        Command = handler.attribute.Group,
                        Subcommand = handler.attribute.Name,
                        Handler = handler.handler
                    };
                })
                .GroupBy(
                    x => x.Command,
                    x => new
                    {
                        x.Subcommand,
                        x.Handler
                    }
                )
                .ToDictionary(
                    x=>x.Key, 
                    x => x.ToDictionary(
                        y=>y.Subcommand, 
                        y=>y.Handler
                        )
                    );
        }

        public async Task DecodeAndDispatch(string channel, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            var parts = text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0];
            if (!_supportedCommands.ContainsKey(command))
                return;

            var subCommands = _supportedCommands[command];

            if (subCommands.Count == 0)
                return;

            if (subCommands.Count > 1)
            {
                if (parts.Length >= 2)
                {
                    var subcommandName = parts[1];
                    var subCommand = subCommands[subcommandName];
                    await subCommand.Invoke(channel, parts.Skip(2).ToArray());
                }
            }

            if (subCommands.Count == 1)
            {
                var c = subCommands.First();
                if (string.IsNullOrWhiteSpace(c.Key))
                {
                    await c.Value.Invoke(channel, parts.Skip(1).ToArray());
                }
                else
                {
                    var subcommandName = parts[1].Trim();
                    if (c.Key.Equals(subcommandName, StringComparison.OrdinalIgnoreCase))
                    {
                        await c.Value.Invoke(channel, parts.Skip(2).ToArray());
                    }
                }
            }
        }
    }

    public interface ISlackCommandDispatcher
    {
        Task DecodeAndDispatch(string channel, string text);
    }
}