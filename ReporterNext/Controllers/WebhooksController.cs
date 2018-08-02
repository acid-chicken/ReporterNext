using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreTweet;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ReporterNext.Models;

namespace ReporterNext.Controllers
{
    [Route("[controller]"), ApiController]
    public class WebhooksController : ControllerBase
    {
        private IConfiguration _configuration;

        public WebhooksController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET webhooks/twitter
        [HttpGet("[action]")]
        public IActionResult Twitter([FromQuery(Name = "crc_token")] string crcToken, [FromQuery(Name = "nonce")] string nonce) =>
            crcToken is null ? NoContent() : Ok(new CRCResponse(_configuration["Twitter:ConsumerSecret"], crcToken)) as IActionResult;

        // POST webhooks/twitter
        [HttpPost("[action]")]
        public IActionResult Twitter([FromBody] EventObject eventObject)
        {
            if (eventObject is null)
                return BadRequest();
            BackgroundJob.Enqueue(() => eventObject.Build());
            return Ok();
        }
    }
}
