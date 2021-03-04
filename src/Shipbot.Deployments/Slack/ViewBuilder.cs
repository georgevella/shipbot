using System.Collections.Generic;
using System.Linq;
using Shipbot.Applications;
using Slack.NetStandard;
using Slack.NetStandard.Messages.Blocks;
using Slack.NetStandard.Messages.Elements;
using Slack.NetStandard.Objects;

namespace Shipbot.Deployments.Slack
{
    public static class ViewBuilder
    {
        public static View BuildManageDeploymentView(IApplicationService applicationService, ViewState? payloadState,
            bool b, bool b1, bool b2)
        {
            var blocks = new List<IMessageBlock>();

            if (b)
            {
                blocks.Add(new Section("Application")
                {
                    BlockId = "app-name-selection",
                    Accessory = new StaticSelect
                    {
                        Placeholder   = "Name of application",
                        ActionId = "app-name-selection",
                        Options = applicationService.GetApplications()
                            .Select( x=> new Option() { Text = new PlainText(x.Name), Value = x.Name } )
                            .ToArray()
                    }
                });
            }

            if (b1)
            {
                if (payloadState != null)
                {
                    var opt = ((Option) payloadState.Values["app-name-selection"]["app-name-selection"].SelectedOption);

                    var app = applicationService.GetApplication(opt.Value);
                    var options = app.Images.Select(x => new Option()
                    {
                        Text = new PlainText($"{x.Repository}({x.TagProperty.Path})"),
                        Value = $"{x.Repository}:{x.TagProperty.Path}"
                    }).ToArray();
                
                    blocks.Add(new Section("Repository")
                    {
                        BlockId = "repository-selection", 
                        Accessory = new StaticSelect
                        {
                            Placeholder   = "Repository",
                            ActionId = "repository-selection",
                            Options = options
                        }
                    });
                }
            }

            if (b2)
            {
                blocks.Add(new Section("Image Tag")
                {
                    BlockId = "tag-selection",
                    Accessory = new ExternalSelect()
                    {
                        Placeholder   = "Tag",
                        ActionId = "tag-selection",
                        MinimumQueryLength = 0
                    }
                });
            }

            return new View()
            {
                Type = "modal",
                Title = "Update Deployment",
                Blocks = blocks.ToArray(),
                CallbackId = "abc",
                Submit = new PlainText("Update")
                // ClearOnClose = true,
                // NotifyOnSubmit = true
            };
        }
    }
}