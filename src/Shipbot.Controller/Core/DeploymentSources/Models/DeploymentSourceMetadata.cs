using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.DeploymentSources.Models
{
    public class DeploymentSourceMetadata
    {
        public HashSet<DeploymentSourceValuePathMetadata> ImageTags { get; set; } = new HashSet<DeploymentSourceValuePathMetadata>(new DeploymentSourceValuePathEqualityComparer());
    }

    public class DeploymentSourceValuePathMetadata
    {
        public string File { get; }
        
        public string ValuePath { get; }
        
        public string Value { get; }

        [JsonConstructor]
        public DeploymentSourceValuePathMetadata(string valuePath, string value, string file)
        {
            File = file;
            Value = value;
            ValuePath = valuePath;
        }
    }
    
    public class FileInfoConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var path = reader.ReadAsString();
            return new FileInfo(path);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((FileInfo)value).FullName);
        }
    }

    public class DeploymentSourceValuePathEqualityComparer : IEqualityComparer<DeploymentSourceValuePathMetadata>
    {
        public DeploymentSourceValuePathEqualityComparer()
        {
            
        }
        
        public bool Equals(DeploymentSourceValuePathMetadata x, DeploymentSourceValuePathMetadata y)
        {
            switch (x)
            {
                case null when y == null:
                    return true;
                case null:
                    return false;
            }

            if (y == null) return false;

            return ReferenceEquals(x, y) || x.ValuePath.Equals(y.ValuePath);
        }

        public int GetHashCode(DeploymentSourceValuePathMetadata obj)
        {
            return obj.ValuePath.GetHashCode();
        }
    }
}