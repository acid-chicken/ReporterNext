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

    public class EventObserver : IObserver<Event>
    {
        private long _forUserId;
        private EventObservableFactory _factory;

        public EventObserver(long forUserId, EventObservableFactory factory)
        {
            _forUserId = forUserId;
            _factory = factory;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(Event value)
        {
            switch (value)
            {
                case TweetCreateEvent x: _factory.Create<TweetCreateEvent>(_forUserId).Execute(x); break;
                case FavoriteEvent x: _factory.Create<FavoriteEvent>(_forUserId).Execute(x); break;
                case FollowEvent x: _factory.Create<FollowEvent>(_forUserId).Execute(x); break;
                case BlockEvent x: _factory.Create<BlockEvent>(_forUserId).Execute(x); break;
                case MuteEvent x: _factory.Create<MuteEvent>(_forUserId).Execute(x); break;
                case UserRevokeEvent x: _factory.Create<UserRevokeEvent>(_forUserId).Execute(x); break;
                case DirectMessageEvent x: _factory.Create<DirectMessageEvent>(_forUserId).Execute(x); break;
                case DirectMessageIndicateTypingEvent x: _factory.Create<DirectMessageIndicateTypingEvent>(_forUserId).Execute(x); break;
                case DirectMessageMarkReadEvent x: _factory.Create<DirectMessageMarkReadEvent>(_forUserId).Execute(x); break;
                case TweetDeleteEvent x: _factory.Create<TweetDeleteEvent>(_forUserId).Execute(x); break;
            }
        }
    }

    public static partial class ServiceCollectionServiceExtensions
    {
        public static IServiceCollection AddReactiveInterface(this IServiceCollection services)
        {
            services.AddSingleton<EventObservableFactory>();

            return services;
        }

        public static IApplicationBuilder UseReactiveInterface(this IApplicationBuilder app, long forUserId = default)
        {
            var undisposings = app.ApplicationServices.GetService<UndisposingObjectCollection>();
            var tokens = app.ApplicationServices.GetService<Tokens>();
            var factory = app.ApplicationServices.GetService<EventObservableFactory>();

            var replyObserver = new ReplyQuotedTimeObserver(forUserId, tokens);
            undisposings.Add(factory.Create<TweetCreateEvent>(forUserId).Subscribe(replyObserver));

            var eventObserver = new EventObserver(forUserId, factory);
            undisposings.Add(factory.Create<Event>(forUserId).Subscribe(eventObserver));

            return app;
        }
    }
}
