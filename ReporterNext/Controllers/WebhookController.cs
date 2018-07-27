using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ReporterNext.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        // POST api/values
        [HttpPost("[action]")]
        public void Twitter([FromBody] JToken value)
        {
            Console.WriteLine(value.ToString());
        }
    }
}
