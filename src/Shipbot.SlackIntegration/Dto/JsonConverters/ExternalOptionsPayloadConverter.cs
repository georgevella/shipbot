using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shipbot.SlackIntegration.Dto.Slack.ExternalOptions;
using Slack.NetStandard.Interaction;

namespace Shipbot.SlackIntegration.Dto.JsonConverters
{
    public class ExternalOptionsPayloadConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var target = GetPayloadType(ToEnum(jObject.Value<string>("type")));
            serializer.Populate(jObject.CreateReader(), target);
            return target;
        }
        
        private ExternalOptionsPayload GetPayloadType(ExternalOptionsType value)
        {
            return value switch
            {
                ExternalOptionsType.BlockSuggestion => new BlockSuggestionPayload(),
                _ => throw new InvalidOperationException()
            };
        }
        
        private static ExternalOptionsType ToEnum(string str)
        {
            var enumType = typeof(ExternalOptionsType);
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new InvalidOperationException();
            }

            foreach (var name in Enum.GetNames(enumType))

            {
                var enumMemberAttribute = ((EnumMemberAttribute[])enumType.GetTypeInfo().GetField(name).GetCustomAttributes(typeof(EnumMemberAttribute), true)).FirstOrDefault();
                if (enumMemberAttribute != null && enumMemberAttribute.Value == str)
                    return (ExternalOptionsType)Enum.Parse(enumType, name);
            }

            throw new InvalidOperationException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ExternalOptionsPayload).IsAssignableFrom(objectType);
        }
    }
}