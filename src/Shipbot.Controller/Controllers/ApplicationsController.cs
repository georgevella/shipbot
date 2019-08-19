using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Shipbot.Controller.Core.Apps;
using Shipbot.Controller.Core.Models;

namespace Shipbot.Controller.Controllers
{
    [Route("api/applications/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IApplicationService _applicationService;

        public ValuesController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }
        
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<Application>> Get()
        {
            return Ok(_applicationService.GetApplications());
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
