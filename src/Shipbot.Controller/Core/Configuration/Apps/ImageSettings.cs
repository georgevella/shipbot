using System;
using Shipbot.Controller.Core.Apps.Models;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Core.Configuration.Apps
{
    public class ImageSettings
    {
        public string Pattern { get; set; }

        public string Repository { get; set; }

        public TagPropertySettings TagProperty { get; set; }

        public UpdatePolicy Policy { get; set; }
    }

    public class TagPropertySettings
    {
        public string Path { get; set; }

        public TagPropertyValueFormat ValueFormat { get; set; }
    }
}