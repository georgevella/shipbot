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

        public IMessage Build()
        {
            return new SlackMessage(_message, _blocks.ToArray());
        }
    }
}