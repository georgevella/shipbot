using System;

namespace Shipbot.SlackIntegration.ExternalOptions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SlackExternalOptionsAttribute : Attribute
    {
        public string ActionId { get; }

        public SlackExternalOptionsAttribute(string actionId)
        {
            ActionId = actionId;
        }
    }
}