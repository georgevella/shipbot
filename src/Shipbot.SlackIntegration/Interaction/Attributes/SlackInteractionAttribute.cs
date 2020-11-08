using System;

namespace Shipbot.SlackIntegration.Interaction
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SlackInteractionAttribute : Attribute
    {
        public string CallbackId { get; }

        public SlackInteractionAttribute(string callbackId)
        {
            CallbackId = callbackId;
        }
    }
}