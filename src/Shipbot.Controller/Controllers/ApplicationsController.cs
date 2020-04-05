// using System.Collections.Generic;
// using Microsoft.AspNetCore.Mvc;
// using Orleans;
// using Shipbot.Controller.Core.Apps;
// using Shipbot.Controller.Core.Apps.Models;
// using Shipbot.Controller.Core.Models;
//
// namespace Shipbot.Controller.Controllers
// {
//     [Route("api/applications/")]
//     [ApiController]
//     public class ValuesController : ControllerBase
//     {
//         private readonly IGrainFactory _grainFactory;
//
//         public ValuesController(IGrainFactory grainFactory)
//         {
//             _grainFactory = grainFactory;
//         }
//         
//         // GET api/values
//         [HttpGet("{application}/{environment}")]
//         public ActionResult<IEnumerable<Application>> Get(string application, string environment)
//         {
//             var environmentGrain = _grainFactory.GetEnvironment(application, environment);
//             
//             environmentGrain.
//         }
//
//         // GET api/values/5
//         [HttpGet("{id}")]
//         public ActionResult<string> Get(int id)
//         {
//             return "value";
//         }
//
//         // POST api/values
//         [HttpPost]
//         public void Post([FromBody] string value)
//         {
//         }
//
//         // PUT api/values/5
//         [HttpPut("{id}")]
//         public void Put(int id, [FromBody] string value)
//         {
//         }
//
//         // DELETE api/values/5
//         [HttpDelete("{id}")]
//         public void Delete(int id)
//         {
//         }
//     }
// }
