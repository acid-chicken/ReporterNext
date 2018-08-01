using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using ReporterNext.Models;

namespace ReporterNext.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WebhooksController : ControllerBase
    {
        private IConfiguration _configuration;

        public WebhooksController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET webhooks/twitter
        [HttpGet("[action]")]
        public IActionResult Twitter([FromQuery(Name = "crc_token")] string crcToken) =>
            crcToken is null ? NoContent() : Ok(new CRCResponse(_configuration["Twitter:ConsumerSecret"], crcToken)) as IActionResult;

        // POST webhooks/twitter
        [HttpPost("[action]")]
        public void Twitter([FromBody] Event value)
        {
            BackgroundJob.Enqueue(() => Console.WriteLine(value));
        }
    }
}
