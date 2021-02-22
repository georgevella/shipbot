using FluentAssertions;
using Shipbot.Applications.Models;
using Xunit;

namespace Shipbot.Tests
{
    public class ImageUpdatePolicyTests
    {
        [Theory]
        [InlineData("^2.0.0", "1.2.0", false)]
        [InlineData("^2.0.0", "2.2.0", true)]
        [InlineData("^2.0.0", "2.0.0", true)]
        [InlineData("^2.0.0", "3.0.0", false)]
        [InlineData("^2.1.0", "2.0.0", false)]
        [InlineData("^2.1.0", "2.1.0", true)]
        [InlineData("^2.1.0", "2.1.1", true)]
        [InlineData("^2.1.0", "2.2.0", true)]
        [InlineData("2.1.0", "2.1.0", true)]
        [InlineData("2.1.0", "2.2.0", false)]
        [InlineData("^0.2.0", "0.2.0", true)]
        [InlineData("^0.2.0", "0.2.5", true)]
        [InlineData("^0.2.0", "0.3.0", false)]
        [InlineData("^0.2.0", "0.1.0", false)]
        [InlineData("^0.1.0", "0.1.0-beta.1", false)]
        [InlineData("^0.1.0-beta.1", "0.1.0-beta.1", true)]
        [InlineData("^0.1.0-beta.1", "0.1.0-beta.2", true)]
        [InlineData("^0.1.0-beta.1", "0.1.0", true)]
        [InlineData("^0.1.0-beta.1", "0.1.5", true)]
        [InlineData("^0.1.0-beta.1", "0.1.5-beta.2", false)]
        [InlineData("^1.2.3-beta.1", "1.2.3-beta.2", true)]
        [InlineData(">=2.2.0", "2.2.0", true)]
        [InlineData(">2.2.1", "2.2.0", false)]
        public void SemanticVersionTest(string pattern, string targetVersion, bool result)
        {
            var policy = new SemverImageUpdatePolicy(pattern);
            policy.IsMatch(targetVersion).Should().Be(result);
        }
    }
}