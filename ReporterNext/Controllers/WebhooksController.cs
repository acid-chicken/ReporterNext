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
    [Route("[controller]/[action]"), ApiController]
    public class WebhooksController : ControllerBase
    {
        private JsonObservable _observable;

        private CRC _crc;

        public WebhooksController(JsonObservable observable, CRC crc)
        {
            _observable = observable;
            _crc = crc;
        }

        // GET webhooks/twitter
        [HttpGet]
        public IActionResult Twitter([FromQuery(Name = "crc_token")] string crcToken, [FromQuery(Name = "nonce")] string nonce) =>
            Ok(_crc.GenerateResponse(crcToken));

        // POST webhooks/twitter
        [HttpPost]
        public IActionResult Twitter([FromBody] EventObject eventObject)
        {
            if (eventObject is null)
                return BadRequest();
            BackgroundJob.Enqueue(() => _observable.Execute(eventObject));
            return Ok();
        }
    }
}
