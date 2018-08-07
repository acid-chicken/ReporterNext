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
            {
                switch (@event)
                {
                    case TweetCreateEvent x:
                        BackgroundJob.Enqueue(() => factory
                            .Create<TweetCreateEvent>(forUserId)
                            .Execute(x));
                        break;
                    case FavoriteEvent x:
                        BackgroundJob.Enqueue(() => factory
                            .Create<FavoriteEvent>(forUserId)
                            .Execute(x));
                        break;
                    case FollowEvent x:
                        BackgroundJob.Enqueue(() => factory
                            .Create<FollowEvent>(forUserId)
                            .Execute(x));
                        break;
                    case BlockEvent x:
                        BackgroundJob.Enqueue(() => factory
                            .Create<BlockEvent>(forUserId)
                            .Execute(x));
                        break;
                    case MuteEvent x:
                        BackgroundJob.Enqueue(() => factory
                            .Create<MuteEvent>(forUserId)
                            .Execute(x));
                        break;
                    case UserRevokeEvent x:
                        BackgroundJob.Enqueue(() => factory
                            .Create<UserRevokeEvent>(forUserId)
                            .Execute(x));
                        break;
                    case DirectMessageEvent x:
                        BackgroundJob.Enqueue(() => factory
                            .Create<DirectMessageEvent>(forUserId)
                            .Execute(x));
                        break;
                    case DirectMessageIndicateTypingEvent x:
                        BackgroundJob.Enqueue(() => factory
                            .Create<DirectMessageIndicateTypingEvent>(forUserId)
                            .Execute(x));
                        break;
                    case DirectMessageMarkReadEvent x:
                        BackgroundJob.Enqueue(() => factory
                            .Create<DirectMessageMarkReadEvent>(forUserId)
                            .Execute(x));
                        break;
                    case TweetDeleteEvent x:
                        BackgroundJob.Enqueue(() => factory
                            .Create<TweetDeleteEvent>(forUserId)
                            .Execute(x));
                        break;
                }
            }
            return Ok();
        }
    }
}
