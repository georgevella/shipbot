using System;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Shipbot.Controller.Core.ApplicationSources;
using Xunit;
using YamlDotNet.RepresentationModel;

namespace Shipbot.Tests
{
    public class YamlFileTests
    {
        [Fact]
        public void SetValueInDoc_SingleDocument()
        {
            var x = @"cat:
  image:
    tag: develop-2892
dog:
  image:
    tag: develop-2892
sheep:
  image:
    tag: develop-10";

            var expected = @"cat:
  image:
    tag: develop-2892
dog:
  image:
    tag: develop-999
sheep:
  image:
    tag: develop-10
";

            var byteArray = Encoding.UTF8.GetBytes(x);
            var inStream = new MemoryStream(byteArray);

            var yamlUtilities = new YamlUtilities();
            var yaml = new YamlStream();
            yamlUtilities.ReadYamlStream(yaml, inStream);

            var doc = yaml.Documents.First();
            yamlUtilities.SetValueInDoc("dog.image.tag", doc, "develop-999");

            var outStream = new MemoryStream();
            yamlUtilities.WriteYamlStream(yaml, outStream);

            outStream.Position = 0;
            var reader = new StreamReader(outStream);
            var text = reader.ReadToEnd();

            text.Should().Be(expected);

        }
        
        [Fact]
        public void SetValueInDoc_MultiDocument()
        {
            var x = @"cat:
  image:
    tag: develop-2892
dog:
  image:
    tag: develop-2892
---
sheep:
  image:
    tag: develop-10";

            var expected = @"cat:
  image:
    tag: develop-2892
dog:
  image:
    tag: develop-999
---
sheep:
  image:
    tag: develop-10
";

            var byteArray = Encoding.UTF8.GetBytes(x);
            var inStream = new MemoryStream(byteArray);

            var yamlUtilities = new YamlUtilities();
            var yaml = new YamlStream();
            yamlUtilities.ReadYamlStream(yaml, inStream);

            var doc = yaml.Documents.First();
            yamlUtilities.SetValueInDoc("dog.image.tag", doc, "develop-999");

            var outStream = new MemoryStream();
            yamlUtilities.WriteYamlStream(yaml, outStream);

            outStream.Position = 0;
            var reader = new StreamReader(outStream);
            var text = reader.ReadToEnd();

            text.Should().Be(expected);

        }
    }
}