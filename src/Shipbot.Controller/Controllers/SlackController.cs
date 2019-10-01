using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shipbot.Controller.Models.Slack;

namespace Shipbot.Controller.Controllers
{
    [Route("slack/interaction/")]
    [ApiController]
    public class SlackController : ControllerBase
    {
        [HttpPost("actions")]
        public ActionResult Action([FromForm(Name = "payload")] string payload)
        {
            JsonConvert.DeserializeObject<ActionPayload>(payload);
            return Ok();
        }
    }
//    public partial class Payload
//    {
//        public static Payload FromJson(string json) => JsonConvert.DeserializeObject<Payload>(json, QuickType.Converter.Settings);
//    }
}