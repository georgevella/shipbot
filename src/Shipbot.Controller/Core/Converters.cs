// using System;
// using System.IO;
// using System.Text;
// using System.Text.Json;
// using System.Text.Json.Serialization;
// using Newtonsoft.Json;
// using Shipbot.Controller.DTOs;
// using JsonConverter = System.Text.Json.Serialization.JsonConverter;
//
// namespace Shipbot.Controller.Core
// {
//     public class SlackIncomingPayloadConverterFactory : JsonConverterFactory
//     {
//         private class Converter : System.Text.Json.Serialization.JsonConverter<BaseSlackIncomingRequestPayload>
//         {
//             public override BaseSlackIncomingRequestPayload Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//             {
//                 if (reader.TokenType != JsonTokenType.StartObject)
//                 {
//                     throw new InvalidOperationException();
//                 }
//                 
//                 using var stream = new MemoryStream();
//                 
//                 using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true))
//                 using (var jsonWriter = new JsonTextWriter(writer))
//                 {
//                     jsonWriter.WriteStartObject();
//
//                     while (reader.Read())
//                     {
//                         switch (reader.TokenType)
//                         {
//                             case JsonTokenType.None:
//                                 break;
//                             case JsonTokenType.StartObject:
//                                 jsonWriter.WriteStartObject();
//                                 break;
//                             case JsonTokenType.EndObject:
//                                 jsonWriter.WriteEndObject();
//                                 break;
//                             case JsonTokenType.StartArray:
//                                 jsonWriter.WriteStartArray();
//                                 break;
//                             case JsonTokenType.EndArray:
//                                 jsonWriter.WriteEndArray();
//                                 break;
//                             case JsonTokenType.PropertyName:
//                                 jsonWriter.WritePropertyName(reader.GetString());
//                                 break;
//                             case JsonTokenType.Comment:
//                                 jsonWriter.WriteComment(reader.GetComment());
//                                 break;
//                             case JsonTokenType.String:
//                                 jsonWriter.WriteValue(reader.GetString());
//                                 break;
//                             case JsonTokenType.Number:
//                                 // TODO: handle doubles and shorts
//                                 jsonWriter.WriteValue(reader.GetInt32());
//                                 break;
//                             case JsonTokenType.True:
//                                 jsonWriter.WriteValue(reader.GetBoolean());
//                                 break;
//                             case JsonTokenType.False:
//                                 jsonWriter.WriteValue(reader.GetBoolean());
//                                 break;
//                             case JsonTokenType.Null:
//                                 jsonWriter.WriteNull();
//                                 break;
//                             default:
//                                 throw new ArgumentOutOfRangeException();
//                         }
//                         
//                         jsonWriter.Flush();
//                         writer.Flush();
//                     }
//                 }
//                 
//                 stream.Position = 0;
//
//                 using (var jsonReader = new JsonTextReader(new StreamReader(stream)))
//                 {
//                     var result = new NewtonsoftJsonSlackIncomingPayloadConverter().ReadJson(jsonReader, typeToConvert, null,
//                         Newtonsoft.Json.JsonSerializer.Create());
//
//                     return (BaseSlackIncomingRequestPayload)result;
//                 }
//             }
//
//             public override void Write(Utf8JsonWriter writer, BaseSlackIncomingRequestPayload value, JsonSerializerOptions options)
//             {
//                 throw new NotImplementedException();
//             }
//         }
//         
//
//         public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
//         {
//             return new Converter();
//         }
//
//         public override bool CanConvert(Type typeToConvert)
//         {
//             return typeToConvert == typeof(BaseSlackIncomingRequestPayload);
//         }
//     }
// }