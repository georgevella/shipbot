using System;
using System.Collections.Generic;
using System.Linq;
using Shipbot.SlackIntegration.Internal;
using SlackAPI;

namespace Shipbot.SlackIntegration
{
    public class SlackMessageBuilder
    {
        private readonly string _message;
        private readonly List<IBlock> _blocks = new List<IBlock>();

        public SlackMessageBuilder(string message)
        {
            _message = message;
        }


        public SlackMessageBuilder AddSection(string? text = null, IEnumerable<string>? fields = null, string? blockId = null)
        {
            var block = new SectionBlock();

            if (text != null) block.text = new Text()
            {
                text = text,
                type = "mrkdwn"
            };

            var fieldsList = fields?.ToList() ?? new List<string>();
            
            if (fieldsList?.Any() ?? false)
                block.fields = fieldsList
                    .Select(x => new Text()
                    {
                        text = x,
                        type = "mrkdwn"
                    })
                    .ToArray();
            
            if (!string.IsNullOrEmpty(blockId)) 
                block.block_id = blockId;
            
            _blocks.Add(block);

            return this;
        }

        public SlackMessageBuilder AddDivider()
        {
            _blocks.Add(new DividerBlock());

            return this;
        }

        public SlackMessageBuilder AddActions(Action<ActionBlockBuilder> c, string? blockId = null)
        {
            var builder  = new ActionBlockBuilder(blockId);
            c(builder);
            _blocks.Add(builder.GetBlock());
            
            return this;
        }

        public IMessage Build()
        {
            return new SlackMessage(_message, _blocks.ToArray());
        }
    }
}

    public class ActionBlockBuilder
    {
        private readonly string? _blockId;
        private readonly List<IElement> _elements = new List<IElement>();

        public ActionBlockBuilder(string? blockId)
        {
            _blockId = blockId;
        }

        public ActionBlockBuilder AddButton(string actionId, string text, string value, string? style = null)
        {
            var element = new ButtonElement()
            {
                action_id = actionId,
                text = new Text()
                {
                    text = text
                },
                value = value
            };

            if (style != null)
            {
                element.style = style;
            }

            _elements.Add(element);

            return this;
        }

        internal ActionsBlock GetBlock()
        {
            return new ActionsBlock()
            {
                block_id = _blockId,
                elements = _elements.ToArray()
            };
        }
    }