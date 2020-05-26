using Hangfire;
using Microsoft.AspNetCore.Mvc;
using ReporterNext.Components;
using ReporterNext.Models;

namespace ReporterNext.Controllers
{
  [Route("[controller]/[action]"), ApiController]
    public class WebhooksController : ControllerBase
    {
        private readonly JsonObservable _observable;

        private readonly CRC _crc;

        public WebhooksController(JsonObservable observable, CRC crc)
        {
            _observable = observable;
            _crc = crc;
        }

        // GET webhooks/twitter
        [HttpGet]
        public IActionResult Twitter(
            [FromQuery(Name = "crc_token")] string crcToken,
#pragma warning disable IDE0060
            [FromQuery(Name = "nonce")] string nonce
#pragma warning restore IDE0060
        ) =>
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
