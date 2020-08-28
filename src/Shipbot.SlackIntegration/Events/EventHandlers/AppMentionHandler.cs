using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Shipbot.SlackIntegration.Commands;
using Slack.NetStandard.EventsApi.CallbackEvents;

namespace Shipbot.SlackIntegration.Events.EventHandlers
{
    public class AppMentionHandler : BaseSlackEventHandler<AppMention>
    {
        private readonly ISlackClient _slackClient;
        private readonly ISlackCommandDispatcher _slackCommandDispatcher;
        
        private static readonly Regex FormattingFilter = new Regex(@"<(?<type>[@#!])?(?<link>[^>|]+)(?:\|(?<label>[^>]+))?>");

        public AppMentionHandler(
            ISlackClient slackClient, 
            ISlackCommandDispatcher slackCommandDispatcher
            )
        {
            _slackClient = slackClient;
            _slackCommandDispatcher = slackCommandDispatcher;
        }
        
        private string RemoveFormatting(string text)
        {
            text = FormattingFilter.Replace(text, m =>
            {

                switch (m.Groups["type"].Value)
                {
                    // case "@":
                    //     if (m.Groups["label"].Success) return m.Groups["label"].Value;
                    //
                    //     var user = _users.SingleOrDefault(u => m.Groups["link"].Value == u.Id);
                    //     if (user != null) return user.Name;
                    //     break;
                    // case "#":
                    //     if (m.Groups["label"].Success) return m.Groups["label"].Value;
                    //
                    //     var channel = _rooms.SingleOrDefault(r => m.Groups["link"].Value == r.Id);
                    //     if (channel != null) return channel.Name;
                    //     break;
                    case "!":
                        string[] links = {"channel","group","everyone","here"};
                        if(links.Contains(m.Groups["link"].Value))
                        {
                            return $"@{m.Groups["link"].Value}";
                        }
                        break;
                    default:
                        string link = m.Groups["link"].Value.Replace("mailto:", "");
                        if (link == m.Groups["label"].Value)
                        {
                            return $"{m.Groups["label"].Value} ({link})";
                        }
                        else
                        {
                            return m.Groups["link"].Value;
                        }
                        break;
                }

                return m.Value;

            });

            return text;

        }

        protected override async Task Invoke(AppMention callbackEvent)
        {
            if (callbackEvent.Text.Contains("hello"))
            {
                await _slackClient.SendMessage(callbackEvent.Channel, "hello back");
                return;
            }

            // var filter = 0;
            // var buffer = new StringBuilder();
            //
            // foreach (var item in callbackEvent.Text)
            // {
            //     switch (item)
            //     {
            //         case '<':
            //             filter++;
            //             break;
            //         case '>':
            //             filter--;
            //             break;
            //         default:
            //             if (filter == 0)
            //             {
            //                 buffer.Append(item);
            //             }
            //
            //             break;
            //     }
            // }
            
            
            var actualMessage = RemoveFormatting(callbackEvent.Text);
            
            // TODO: this handles only message which start with the bot name (ex: '<bot> do something')
            actualMessage = string.Join(
                ' ', 
                actualMessage.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1)
                );
            await _slackCommandDispatcher.DecodeAndDispatch(callbackEvent.Channel, actualMessage);
        }
    }
}