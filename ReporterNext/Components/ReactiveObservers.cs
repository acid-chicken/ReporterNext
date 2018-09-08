using System;
using System.Threading.Tasks;
using CoreTweet;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ReporterNext.Models;

namespace ReporterNext.Components
{
    public class ReplyQuotedTimeObserver : IObserver<TweetCreateEvent>, IObserver<Event>
    {
        private string _consumerKey;
        private string _consumerSecret;
        private string _accessToken;
        private string _accessTokenSecret;

        public ReplyQuotedTimeObserver(Tokens tokens)
        {
            _consumerKey = tokens.ConsumerKey;
            _consumerSecret = tokens.ConsumerSecret;
            _accessToken = tokens.AccessToken;
            _accessTokenSecret = tokens.AccessTokenSecret;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(Event value) =>
            OnNext((TweetCreateEvent)value);

        public void OnNext(TweetCreateEvent value) =>
            BackgroundJob.Enqueue(() => JobAsync(_consumerKey, _consumerSecret, _accessToken, _accessTokenSecret, value));

        public static Task JobAsync(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, TweetCreateEvent @event) =>

            ((@event.Target.QuotedStatusId ?? @event.Target.QuotedStatus?.Id) is long statusId &&
                @event.Target.User.Id is long userId) ?
                Tokens.Create(consumerKey, consumerSecret, accessToken, accessTokenSecret).Statuses.UpdateAsync(
                    status => $"ツイート時刻：{statusId.ToSnowflake().ToOffset(new TimeSpan(9, 0, 0)):HH:mm:ss.fff}",
                    in_reply_to_status_id => userId,
                    auto_populate_reply_metadata => true,
                    tweet_mode => TweetMode.Extended) :
                Task.CompletedTask;
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
            switch (value.CheckType())
            {
                case EventType.TweetCreateEvent when value.TryToTweetCreateEvent(out var forUserId, out var events):
                    foreach (var @event in events)
                        _factory.Create<TweetCreateEvent>(forUserId).Execute(@event); break;
                case EventType.FavoriteEvent when value.TryToFavoriteEvent(out var forUserId, out var events):
                    foreach (var @event in events)
                        _factory.Create<FavoriteEvent>(forUserId).Execute(@event); break;
                case EventType.FollowEvent when value.TryToFollowEvent(out var forUserId, out var events):
                    foreach (var @event in events)
                        _factory.Create<FollowEvent>(forUserId).Execute(@event); break;
                case EventType.BlockEvent when value.TryToBlockEvent(out var forUserId, out var events):
                    foreach (var @event in events)
                        _factory.Create<BlockEvent>(forUserId).Execute(@event); break;
                case EventType.MuteEvent when value.TryToMuteEvent(out var forUserId, out var events):
                    foreach (var @event in events)
                        _factory.Create<MuteEvent>(forUserId).Execute(@event); break;
                case EventType.UserRevokeEvent when value.TryToUserRevokeEvent(out var forUserId, out var events):
                    foreach (var @event in events)
                        _factory.Create<UserRevokeEvent>(forUserId).Execute(@event); break;
                case EventType.DirectMessageEvent when value.TryToDirectMessageEvent(out var forUserId, out var events):
                    foreach (var @event in events)
                        _factory.Create<DirectMessageEvent>(forUserId).Execute(@event); break;
                case EventType.DirectMessageIndicateTypingEvent when value.TryToDirectMessageIndicateTypingEvent(out var forUserId, out var events):
                    foreach (var @event in events)
                        _factory.Create<DirectMessageIndicateTypingEvent>(forUserId).Execute(@event); break;
                case EventType.DirectMessageMarkReadEvent when value.TryToDirectMessageMarkReadEvent(out var forUserId, out var events):
                    foreach (var @event in events)
                        _factory.Create<DirectMessageMarkReadEvent>(forUserId).Execute(@event); break;
                case EventType.TweetDeleteEvent when value.TryToTweetDeleteEvent(out var forUserId, out var events):
                    foreach (var @event in events)
                        _factory.Create<TweetDeleteEvent>(forUserId).Execute(@event); break;
                default:
                    UnknownEvent(nameof(value)); break;
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
                .Subscribe(new ReplyQuotedTimeObserver(tokens), true);

            json.Subscribe(new EventObserver(factory), true);

            return app;
        }
    }
}
