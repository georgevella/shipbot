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
        
        [Theory]
        [InlineData("instances[0].image.tag", "develop-2892")]
        [InlineData("instances[2].field", "xyz")]
        [InlineData("instances[4].field", null)]
        public void GetValue_SingleDocument_ArrayPath(string path, string expected)
        {
            var x = @"
instances:
- field: abc
  image:
    tag: develop-2892
- field: def
  image:
    tag: develop-123
- field: xyz
  image:
    tag: develop-10";

            var byteArray = Encoding.UTF8.GetBytes(x);
            var inStream = new MemoryStream(byteArray);

            var yamlUtilities = new YamlUtilities();
            var yaml = new YamlStream();
            yamlUtilities.ReadYamlStream(yaml, inStream);

            var doc = yaml.Documents.First();
            var readValue = yamlUtilities.ExtractValueFromDoc(path, doc);

            readValue.Should().Be(expected);
        }
        
        [Theory]
        [InlineData("cat[\"image1\"].tag", "develop-2892")]
        [InlineData("cat.image1[tag]", "develop-2892")]
        public void GetValue_SingleDocument_MapPath(string path, string expected)
        {
            var x = @"
cat:
  image1:
    tag: develop-2892
  image2:
    tag: develop-123";

            var byteArray = Encoding.UTF8.GetBytes(x);
            var inStream = new MemoryStream(byteArray);

            var yamlUtilities = new YamlUtilities();
            var yaml = new YamlStream();
            yamlUtilities.ReadYamlStream(yaml, inStream);

            var doc = yaml.Documents.First();
            var readValue = yamlUtilities.ExtractValueFromDoc(path, doc);

            readValue.Should().Be(expected);
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