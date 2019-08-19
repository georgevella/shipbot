using System;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Configuration.Apps
{
    public class ImageSettings
    {
        public string Pattern { get; set; }

        public string Repository { get; set; }

        public TagPropertySettings TagProperty { get; set; }

        public UpdatePolicy Policy { get; set; }

        public static implicit operator Image(ImageSettings imageSettings)
        {
            return new Image(
                imageSettings.Repository,
                new TagProperty(
                    imageSettings.TagProperty.Path,
                    imageSettings.TagProperty.ValueFormat
                ),
                imageSettings.Policy switch
                    {
                    UpdatePolicy.Glob => (ImageUpdatePolicy) new GlobImageUpdatePolicy(
                        imageSettings.Pattern),
                    UpdatePolicy.Regex => new RegexImageUpdatePolicy(imageSettings.Pattern),
                    _ => throw new NotImplementedException()
                    }
            );
        }
    }

    public class TagPropertySettings
    {
        public string Path { get; set; }

        public TagPropertyValueFormat ValueFormat { get; set; }
    }
}