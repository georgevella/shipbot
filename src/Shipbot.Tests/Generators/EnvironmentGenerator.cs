using System.Collections.Generic;
using System.Collections.Immutable;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Tests.Generators
{
    public static class EnvironmentGenerator
    {
        public static ApplicationEnvironment BuildEnvironment(
            string name = "dev", 
            IEnumerable<Image> images = null, 
            bool autoDeploy = true,
            List<string> promotionList = null
            )
        {
            if (images == null)
            {
                images = new[]
                {
                    new Image(
                        "repository/image",
                        new TagProperty("image.tag", TagPropertyValueFormat.TagOnly),
                        new GlobImageUpdatePolicy("develop-*")
                    ),
                };
            }

            return new ApplicationEnvironment(name,
                images.ToImmutableList(),
                new HelmApplicationSource(),
                autoDeploy,
                promotionList ?? new List<string>());
        }
    }
}