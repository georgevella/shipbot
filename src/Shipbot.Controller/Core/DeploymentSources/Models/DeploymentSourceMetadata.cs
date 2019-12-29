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
        public HashSet<ImageTagMetadata> ImageTags { get; set; } = new HashSet<ImageTagMetadata>(new ImageTagSourceFileEqualityComparer());
    }

    public class ImageTagMetadata
    {
        public Image Image { get; set; }
        
        public string File { get; set; }
        
        public string Tag { get; set; }

        public ImageTagMetadata()
        {
            
        }

        public ImageTagMetadata(Image image, string file, string tag)
        {
            Image = image;
            File = file;
            Tag = tag;
        }

        public static implicit operator ImageTagMetadata(Image image)
        {
            return new ImageTagMetadata(image, null, null);
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

    public class ImageTagSourceFileEqualityComparer : IEqualityComparer<ImageTagMetadata>
    {
        public ImageTagSourceFileEqualityComparer()
        {
            
        }
        
        public bool Equals(ImageTagMetadata x, ImageTagMetadata y)
        {
            switch (x)
            {
                case null when y == null:
                    return true;
                case null:
                    return false;
            }

            if (y == null) return false;

            return ReferenceEquals(x, y) || Image.EqualityComparer.Equals(x.Image, y.Image);
        }

        public int GetHashCode(ImageTagMetadata obj)
        {
            return Image.EqualityComparer.GetHashCode(obj.Image);
        }
    }
}