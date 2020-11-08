using System;

namespace Shipbot.SlackIntegration.Interaction
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SlackActionAttribute : Attribute
    {
        public string ActionId { get; }

        public SlackActionAttribute(string actionId)
        {
            ActionId = actionId;
        }
    }
}