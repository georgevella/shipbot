using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shipbot.Controller.DTOs;

namespace Shipbot.Controller.Core
{
    public class SlackIncomingPayloadConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JToken.ReadFrom(reader);
            var type = jObject["type"]?.ToObject<SlackIncomingRequestType>();

            if (type == null)
            {
                throw new InvalidOperationException();
            }
            
            
            BaseSlackIncomingRequestPayload result;
            switch (type)
            {
                case SlackIncomingRequestType.url_verification:
                    result = new UrlVerificationPayload();
                    break;
                case SlackIncomingRequestType.block_actions:
                    result = new BlockActionsPayload();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            serializer.Populate(jObject.CreateReader(), result);

            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}