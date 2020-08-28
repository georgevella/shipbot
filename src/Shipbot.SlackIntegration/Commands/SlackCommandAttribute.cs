using System;

namespace Shipbot.SlackIntegration.Commands
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SlackCommandAttribute : Attribute
    {
        public string Name { get; }
        public string Group { get; }

        public SlackCommandAttribute(string name, string? group = null)
        {
            Name = name;
            Group = group ?? string.Empty;
        }
    }
}