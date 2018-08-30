using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreTweet;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ReporterNext.Components;
using ReporterNext.Models;

namespace ReporterNext.Controllers
{
    [Route("[controller]"), ApiController]
    public class WebhooksController : ControllerBase
    {
        private IConfiguration _configuration;

        private CRC _crc;

        public WebhooksController(IConfiguration configuration, CRC crc)
        {
            _configuration = configuration;
            _crc = crc;
        }

        // GET webhooks/twitter
        [HttpGet("[action]")]
        public IActionResult Twitter([FromQuery(Name = "crc_token")] string crcToken, [FromQuery(Name = "nonce")] string nonce) =>
            Ok(_crc.GenerateResponse(crcToken));

        // POST webhooks/twitter
        [HttpPost("[action]")]
        public IActionResult Twitter([FromBody] EventObject eventObject)
        {
            if (eventObject is null)
                return BadRequest();
            var (forUserId, events) = eventObject.Build();
            var factory = _configuration.Get<EventObservableFactory>();
            foreach (var @event in events)
                factory.Create(forUserId).Execute(@event);
            return Ok();
        }
    }
}
