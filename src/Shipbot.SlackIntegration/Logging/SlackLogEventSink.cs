using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Options;
using Serilog.Core;
using Serilog.Events;
using Shipbot.Controller.Core.Configuration;

namespace Shipbot.SlackIntegration.Logging
{
    public class SlackLogEventSink : ILogEventSink
    {
        private readonly HttpClient _httpClient;
        private readonly string _slackWebhookUrl;

        public SlackLogEventSink(string slackWebhookUrl)
        {
            _slackWebhookUrl = slackWebhookUrl;
            _httpClient = new HttpClient();
        }

        public void Emit(LogEvent logEvent)
        {
            string color;

            switch (logEvent.Level)
            {
                case LogEventLevel.Error:
                case LogEventLevel.Fatal:
                    color = "danger";
                    break;
                case LogEventLevel.Warning:
                    color = "warning";
                    break;
                default:
                    color = "good";
                    break;
            }

            var title = logEvent.RenderMessage() ?? (logEvent.MessageTemplate.Text ?? "<Message unavailable>");
            
            var ex = logEvent.Exception;

            var sourceContextStringWriter = new StringWriter();

            var sourceContextProperty = logEvent.Properties.GetValueOrDefault("SourceContext");
            if (sourceContextProperty != null)
            {
                sourceContextProperty.Render(sourceContextStringWriter);
            }
            else
            {
                sourceContextStringWriter.Write("n/a");
            }

            string GetStackTraceAsMarkDown(Exception e)
            {
                var buffer = new StringBuilder();
                buffer.AppendLine($"*Exception Type*: {ex.GetType()}");
                buffer.Append("```");
                buffer.Append(e.StackTrace);
                buffer.Append("```");

                return buffer.ToString();
            }
            
            
            var obj = new
            {
                attachments = new[]
                {
                    new
                    {
                        fallback = $"{logEvent.Level}: {title}",
                        color = color,
                        pretext = $"{logEvent.Level}: {title}",
                        title = ex != null ? $"Exception: {ex.Message} (source: {ex.Source})" : null,
                        text = ex != null ? GetStackTraceAsMarkDown(ex) : null,
                        fields = new[]
                        {
                            new
                            {
                                title = "SourceContext",
                                value = sourceContextStringWriter.ToString(),
                                @short = "true"
                            }
                        }
                    }
                }
            };

            _httpClient.PostAsJsonAsync(_slackWebhookUrl, obj).Wait();
        }
    }
}
