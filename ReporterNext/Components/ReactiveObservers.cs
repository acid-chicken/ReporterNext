using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private readonly string _accessToken;
        private readonly string _accessTokenSecret;

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

        public static Task JobAsync(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, TweetCreateEvent @event)
        {
            var tokens = Tokens.Create(consumerKey, consumerSecret, accessToken, accessTokenSecret);
            var myId = long.Parse(accessToken.Split('-')[0]);

            Task ReplyAsync(long id) =>
                tokens.Statuses.UpdateAsync(
                    status => $"ツイート時刻：{id.ToSnowflake().ToOffset(new TimeSpan(9, 0, 0)):HH:mm:ss.fff}",
                    in_reply_to_status_id => @event.Target.Id,
                    auto_populate_reply_metadata => true,
                    include_ext_alt_text => true,
                    tweet_mode => TweetMode.Extended);

            return
                (@event.Target.QuotedStatusId ?? @event.Target.QuotedStatus?.Id) is long quotedId &&
                @event.Target.InReplyToUserId is long userId &&
                userId == myId ?
                    ReplyAsync(quotedId) :
                @event.Target.InReplyToStatusId is long replyId &&
                @event.Target.User.Id != myId ?
                    ReplyAsync(replyId) :
                Task.CompletedTask;
        }
    }

    public class ReplyDirectMessageQuotedTimeObserver : IObserver<DirectMessageEvent>, IObserver<Event>
    {
        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private readonly string _accessToken;
        private readonly string _accessTokenSecret;

        public ReplyDirectMessageQuotedTimeObserver(Tokens tokens)
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

        public void OnNext(DirectMessageEvent value) =>
            BackgroundJob.Enqueue(() => JobAsync(_consumerKey, _consumerSecret, _accessToken, _accessTokenSecret, value));

        public static Task JobAsync(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, DirectMessageEvent @event)
        {
            var tokens = Tokens.Create(consumerKey, consumerSecret, accessToken, accessTokenSecret);
            var myId = long.Parse(accessToken.Split('-')[0]);
            var ids = @event.Content.Entities.Urls
                .Select(x => new Uri(x.ExpandedUrl))
                .Where(x => x.Host == "twitter.com")
                .Select(x => long.TryParse(x.AbsolutePath
                    .Split('/')
                    .Aggregate(default(string), (a, c) => (a ?? c) == "status" ? c : a), out var result) ? result : default)
                .Where(x => x != default);

            if (!ids.Any() || !(@event.Source.Id is long recipientId))
                return Task.CompletedTask;

            var markReadTask = tokens.DirectMessages.MarkReadAsync(
                last_read_event_id => @event.Id,
                recipient_id => recipientId);

            Task ReplyAsync(long recipientId, long statusId) =>
                tokens.DirectMessages.Events.NewAsync(
                    recipient_id => recipientId,
                    text => $"ツイート時刻：{statusId.ToSnowflake().ToOffset(new TimeSpan(9, 0, 0)):HH:mm:ss.fff}");

            Task ReplyBulkAsync(long recipientId, IEnumerable<long> statusIds) =>
                tokens.DirectMessages.Events.NewAsync(
                    recipient_id => recipientId,
                    text => string.Join('\n', Enumerable
                        .Repeat("ツイート時刻（上から順に）", 1)
                        .Concat(statusIds.Select(x => x.ToSnowflake().ToOffset(new TimeSpan(9, 0, 0)).ToString("HH:mm:ss.fff")))));

            return Task.WhenAll(
                markReadTask,
                ids.Count() == 1 && ids.FirstOrDefault() is long id ?
                    ReplyAsync(recipientId, id) :
                    ReplyBulkAsync(recipientId, ids));
        }
    }

    public class EventObserver : IObserver<EventObject>
    {
        private readonly EventObservableFactory _factory;

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

            factory.Create<DirectMessageEvent>(forUserId)
                .Subscribe(new ReplyDirectMessageQuotedTimeObserver(tokens), true);

            json.Subscribe(new EventObserver(factory), true);

            return app;
        }
    }
}
