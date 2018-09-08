using System;
using CoreTweet;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ReporterNext.Models;

namespace ReporterNext.Components
{
    public class ReplyQuotedTimeObserver : IObserver<TweetCreateEvent>, IObserver<Event>
    {
        private long _forUserId;
        private Tokens _tokens;

        public ReplyQuotedTimeObserver(long forUserId, Tokens tokens)
        {
            _forUserId = forUserId;
            _tokens = tokens;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(Event value) =>
            OnNext((TweetCreateEvent)value);

        public void OnNext(TweetCreateEvent value)
        {
            var nullableId = value.Target.QuotedStatusId ?? value.Target.QuotedStatus?.Id;
            if (nullableId is long id)
            {
                BackgroundJob.Enqueue(() => _tokens.Statuses.UpdateAsync(
                    status => $"ツイート時刻：{id.ToSnowflake().ToOffset(new TimeSpan(9, 0, 0)):HH:mm:ss.fff}",
                    in_reply_to_status_id => value.Target.Id,
                    auto_populate_reply_metadata => true,
                    tweet_mode => TweetMode.Extended));
            }
        }
    }

    public class EventObserver : IObserver<EventObject>
    {
        private EventObservableFactory _factory;

        public EventObserver(EventObservableFactory factory)
        {
            _factory = factory;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(EventObject value)
        {
            var (forUserId, events) = value.Build();
            foreach (var @event in events)
            {
                switch (@event)
                {
                    case TweetCreateEvent x: _factory.Create<TweetCreateEvent>(forUserId).Execute(x); break;
                    case FavoriteEvent x: _factory.Create<FavoriteEvent>(forUserId).Execute(x); break;
                    case FollowEvent x: _factory.Create<FollowEvent>(forUserId).Execute(x); break;
                    case BlockEvent x: _factory.Create<BlockEvent>(forUserId).Execute(x); break;
                    case MuteEvent x: _factory.Create<MuteEvent>(forUserId).Execute(x); break;
                    case UserRevokeEvent x: _factory.Create<UserRevokeEvent>(forUserId).Execute(x); break;
                    case DirectMessageEvent x: _factory.Create<DirectMessageEvent>(forUserId).Execute(x); break;
                    case DirectMessageIndicateTypingEvent x: _factory.Create<DirectMessageIndicateTypingEvent>(forUserId).Execute(x); break;
                    case DirectMessageMarkReadEvent x: _factory.Create<DirectMessageMarkReadEvent>(forUserId).Execute(x); break;
                    case TweetDeleteEvent x: _factory.Create<TweetDeleteEvent>(forUserId).Execute(x); break;
                    default: UnknownEvent(nameof(value)); break;
                }
            }
        }

        private void UnknownEvent(string name) =>
            throw new ArgumentException(name, "Argument cannot be an unknown event");
    }

    public static partial class ServiceCollectionServiceExtensions
    {
        public static IServiceCollection AddReactiveInterface(this IServiceCollection services)
        {
            services.AddSingleton<EventObservableFactory>();
            services.AddSingleton<JsonObservable>();

            return services;
        }

        public static IApplicationBuilder UseReactiveInterface(this IApplicationBuilder app, long forUserId = default)
        {
            var tokens = app.ApplicationServices.GetService<Tokens>();
            var factory = app.ApplicationServices.GetService<EventObservableFactory>();
            var json = app.ApplicationServices.GetService<JsonObservable>();

            factory.Create<TweetCreateEvent>(forUserId)
                .Subscribe(new ReplyQuotedTimeObserver(forUserId, tokens), true);

            json.Subscribe(new EventObserver(factory), true);

            return app;
        }
    }
}
