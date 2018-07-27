using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using ReporterNext.Models;

namespace ReporterNext.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhooksController : ControllerBase
    {
        private IConfiguration _configuration;

        public WebhooksController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET webhook/twitter
        [HttpGet("[action]")]
        public CRCResponse Twitter([FromQuery(Name = "crc_token")] string crcToken)
            => new CRCResponse(_configuration["Twitter:ConsumerSecret"], crcToken);

        // POST webhook/twitter
        [HttpPost("[action]")]
        public void Twitter([FromBody] JToken value)
        {
            Console.Out.WriteLineAsync(value.ToString());
        }
    }
}
