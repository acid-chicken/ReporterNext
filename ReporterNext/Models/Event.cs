using System;
using System.Collections.Generic;
using System.Linq;
using CoreTweet;
using CoreTweet.Core;
using Newtonsoft.Json;

namespace ReporterNext.Models
{
    public interface IEvent
    {
        DateTimeOffset CreatedAt { get; set; }
    }

    public interface ITargetEvent<T> : IEvent
    {
        T Target { get; set; }
    }

    public interface ISourceEvent<T> : IEvent
    {
        T Source { get; set; }
    }

    public abstract class Event : IEvent
    {
        public DateTimeOffset CreatedAt { get; set; }
    }

    [JsonObject]
    public class EventObject
    {
        [JsonProperty("for_user_id")]
        public string ForUserId { get; set; }

        [JsonProperty("tweet_create_events")]
        public Status[] TweetCreateEvents { get; set; }

        [JsonProperty("favorite_events")]
        public FavoriteEventObject[] FavoriteEvents { get; set; }

        [JsonProperty("follow_events")]
        public SourceTargetEventObject<User, User> FollowEvents { get; set; }

        [JsonProperty("block_events")]
        public SourceTargetEventObject<User, User> BlockEvents { get; set; }

        [JsonProperty("mute_events")]
        public SourceTargetEventObject<User, User> MuteEvents { get; set; }

        [JsonProperty("user_event")]
        public UserEventObject UserEvent { get; set; }

        [JsonProperty("direct_message_events")]
        public DirectMessageEventObject[] DirectMessageEvents { get; set; }

        [JsonProperty("direct_message_indicate_typing_events")]
        public DirectMessageIndicateTypingEventObject[] DirectMessageIndicateTypingEvents { get; set; }

        [JsonProperty("direct_message_mark_read_events")]
        public DirectMessageMarkReadEventObject[] DirectMessageMarkReadEvents { get; set; }

        [JsonProperty("tweet_delete_events")]
        public TweetDeleteEventObject[] TweetDeleteEvents { get; set; }

        [JsonProperty("apps")]
        public IDictionary<string, AppObject> Apps { get; set; }

        [JsonProperty("users")]
        public IDictionary<string, UserObject> Users { get; set; }


        public (long forUserId, IEnumerable<Event> events) Build() =>

            (long.TryParse(ForUserId ?? "", out var forUserId) ? forUserId : default,
            !(TweetCreateEvents is null) ?
                TweetCreateEvents.Select(x => new TweetCreateEvent()
                {
                    CreatedAt = x.CreatedAt,
                    Target = x
                }) :
            !(FavoriteEvents is null) ?
                FavoriteEvents.Select(x => new FavoriteEvent()
                {
                    CreatedAt = x.Timestamp,
                    Target = x.FavoritedStatus,
                    Source = x.User
                }) :
            !(FollowEvents is null) ?
                new []
                {
                    new FollowEvent()
                    {
                        CreatedAt = FollowEvents.Timestamp,
                        Target = FollowEvents.Target,
                        Source = FollowEvents.Source
                    }
                } :
            !(BlockEvents is null) ?
                new []
                {
                    new BlockEvent()
                    {
                        CreatedAt = BlockEvents.Timestamp,
                        Target = BlockEvents.Target,
                        Source = BlockEvents.Source
                    }
                } :
            !(MuteEvents is null) ?
                new []
                {
                    new MuteEvent()
                    {
                        CreatedAt = MuteEvents.Timestamp,
                        Target = MuteEvents.Target,
                        Source = MuteEvents.Source
                    }
                } :
            !(UserEvent is null || UserEvent.Revoke is null) ?
                new []
                {
                    new UserRevokeEvent()
                    {
                        Target = long.TryParse(UserEvent.Revoke.Target.AppId ?? "", out var appId) ? appId : default,
                        Source = long.TryParse(UserEvent.Revoke.Source.UserId ?? "", out var userId) ? userId : default
                    }
                } :
            !(DirectMessageEvents is null) ?
                DirectMessageEvents.Select(x => new DirectMessageEvent()
                {
                    CreatedAt = x.Timestamp,
                    Target = Users.TryGetValue(x.MessageCreate.Target.RecipientId, out var target) ? new User()
                    {
                        Id = long.TryParse(target.Id, out var targetId) ? targetId : null as long?,
                        CreatedAt = target.Timestamp,
                        Name = target.Name,
                        ScreenName = target.ScreenName,
                        Location = target.Location,
                        Description = target.Description,
                        IsProtected = target.IsProtected,
                        IsVerified = target.IsVerified,
                        FollowersCount = target.FollowersCount,
                        FriendsCount = target.FriendsCount,
                        StatusesCount = target.StatusesCount,
                        ProfileImageUrl = target.ProfileImageUrl ?? target.ProfileImageUrlHttps.Replace("https://", "http://"),
                        ProfileImageUrlHttps = target.ProfileImageUrlHttps ?? target.ProfileImageUrl.Replace("http://", "https://")
                    } : null,
                    Source = Users.TryGetValue(x.MessageCreate.SenderId, out var source) ? new User()
                    {
                        Id = long.TryParse(source.Id, out var sourceId) ? sourceId : null as long?,
                        CreatedAt = source.Timestamp,
                        Name = source.Name,
                        ScreenName = source.ScreenName,
                        Location = source.Location,
                        Description = source.Description,
                        IsProtected = source.IsProtected,
                        IsVerified = source.IsVerified,
                        FollowersCount = source.FollowersCount,
                        FriendsCount = source.FriendsCount,
                        StatusesCount = source.StatusesCount,
                        ProfileImageUrl = source.ProfileImageUrl ?? source.ProfileImageUrlHttps.Replace("https://", "http://"),
                        ProfileImageUrlHttps = source.ProfileImageUrlHttps ?? source.ProfileImageUrl.Replace("http://", "https://")
                    } : null,
                    App = Apps.TryGetValue(x.MessageCreate.SourceAppId, out var app) ? new App()
                    {
                        Id = app.Id,
                        Name = app.Name,
                        Url = app.Url
                    } : null,
                    Content = x.MessageCreate.MessageData
                }) :
            !(DirectMessageIndicateTypingEvents is null) ?
                DirectMessageIndicateTypingEvents.Select(x => new DirectMessageIndicateTypingEvent()
                {
                    CreatedAt = x.Timestamp,
                    Target = Users.TryGetValue(x.Target.RecipientId, out var target) ? new User()
                    {
                        Id = long.TryParse(target.Id, out var targetId) ? targetId : null as long?,
                        CreatedAt = target.Timestamp,
                        Name = target.Name,
                        ScreenName = target.ScreenName,
                        Location = target.Location,
                        Description = target.Description,
                        IsProtected = target.IsProtected,
                        IsVerified = target.IsVerified,
                        FollowersCount = target.FollowersCount,
                        FriendsCount = target.FriendsCount,
                        StatusesCount = target.StatusesCount,
                        ProfileImageUrl = target.ProfileImageUrl ?? target.ProfileImageUrlHttps.Replace("https://", "http://"),
                        ProfileImageUrlHttps = target.ProfileImageUrlHttps ?? target.ProfileImageUrl.Replace("http://", "https://")
                    } : null,
                    Source = Users.TryGetValue(x.SenderId, out var source) ? new User()
                    {
                        Id = long.TryParse(target.Id, out var sourceId) ? sourceId : null as long?,
                        CreatedAt = source.Timestamp,
                        Name = source.Name,
                        ScreenName = source.ScreenName,
                        Location = source.Location,
                        Description = source.Description,
                        IsProtected = source.IsProtected,
                        IsVerified = source.IsVerified,
                        FollowersCount = source.FollowersCount,
                        FriendsCount = source.FriendsCount,
                        StatusesCount = source.StatusesCount,
                        ProfileImageUrl = source.ProfileImageUrl ?? source.ProfileImageUrlHttps.Replace("https://", "http://"),
                        ProfileImageUrlHttps = source.ProfileImageUrlHttps ?? source.ProfileImageUrl.Replace("http://", "https://")
                    } : null,
                }) :
            !(DirectMessageMarkReadEvents is null) ?
                DirectMessageMarkReadEvents.Select(x => new DirectMessageMarkReadEvent()
                {
                    CreatedAt = x.Timestamp,
                    Target = Users.TryGetValue(x.Target.RecipientId, out var target) ? new User()
                    {
                        Id = long.TryParse(target.Id, out var targetId) ? targetId : null as long?,
                        CreatedAt = target.Timestamp,
                        Name = target.Name,
                        ScreenName = target.ScreenName,
                        Location = target.Location,
                        Description = target.Description,
                        IsProtected = target.IsProtected,
                        IsVerified = target.IsVerified,
                        FollowersCount = target.FollowersCount,
                        FriendsCount = target.FriendsCount,
                        StatusesCount = target.StatusesCount,
                        ProfileImageUrl = target.ProfileImageUrl ?? target.ProfileImageUrlHttps.Replace("https://", "http://"),
                        ProfileImageUrlHttps = target.ProfileImageUrlHttps ?? target.ProfileImageUrl.Replace("http://", "https://")
                    } : null,
                    Source = Users.TryGetValue(x.SenderId, out var source) ? new User()
                    {
                        Id = long.TryParse(source.Id, out var sourceId) ? sourceId : null as long?,
                        CreatedAt = source.Timestamp,
                        Name = source.Name,
                        ScreenName = source.ScreenName,
                        Location = source.Location,
                        Description = source.Description,
                        IsProtected = source.IsProtected,
                        IsVerified = source.IsVerified,
                        FollowersCount = source.FollowersCount,
                        FriendsCount = source.FriendsCount,
                        StatusesCount = source.StatusesCount,
                        ProfileImageUrl = source.ProfileImageUrl ?? source.ProfileImageUrlHttps.Replace("https://", "http://"),
                        ProfileImageUrlHttps = source.ProfileImageUrlHttps ?? source.ProfileImageUrl.Replace("http://", "https://")
                    } : null,
                }) :
            !(TweetDeleteEvents is null) ?
                TweetDeleteEvents.Select(x => new TweetDeleteEvent()
                {
                    CreatedAt = x.Timestamp,
                    Target = long.TryParse(x.Status.Id, out var targetId) ? targetId : default,
                    Source = long.TryParse(x.Status.UserId, out var sourceId) ? sourceId : default,
                }) :
                Enumerable.Empty<Event>());
    }

    public abstract class ToStatusEvent : Event, ITargetEvent<Status>
    {
        public Status Target { get; set; }
    }

    public class TweetCreateEvent : ToStatusEvent
    {
    }

    public class FavoriteEvent : ToStatusEvent, ISourceEvent<User>
    {
        public User Source { get; set; }
    }

    [JsonObject]
    public class FavoriteEventObject
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("created_at"), JsonConverter(typeof(DateTimeOffsetConverter))]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("timestamp_ms"), JsonConverter(typeof(TimestampConverter))]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("favorited_status")]
        public Status FavoritedStatus { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }
    }

    public abstract class UserToUserEvent : Event, ITargetEvent<User>, ISourceEvent<User>
    {
        public User Source { get; set; }

        public User Target { get; set; }
    }

    public class FollowEvent : UserToUserEvent
    {
    }

    public class MuteEvent : UserToUserEvent
    {
    }

    public class BlockEvent : UserToUserEvent
    {
    }

    [JsonObject]
    public class SourceTargetEventObject<TSource, TTarget>
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("created_timestamp"), JsonConverter(typeof(TimestampConverter))]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("source")]
        public TSource Source { get; set; }

        [JsonProperty("target")]
        public TTarget Target { get; set; }
    }

    public class UserRevokeEvent : Event, ITargetEvent<long>, ISourceEvent<long>
    {
        public long Target { get; set; }

        public long Source { get; set; }
    }

    [JsonObject]
    public class UserEventObject
    {
        [JsonProperty("revoke")]
        public UserRevokeEventObject Revoke { get; set; }
    }

    [JsonObject]
    public class UserRevokeEventObject
    {
        [JsonProperty("date_time")]
        public DateTimeOffset DateTime { get; set; }

        [JsonProperty("target")]
        public AppIdObject Target { get; set; }

        [JsonProperty("source")]
        public UserIdObject Source { get; set; }
    }

    [JsonObject]
    public class AppIdObject
    {
        [JsonProperty("app_id")]
        public string AppId { get; set; }
    }

    [JsonObject]
    public class UserIdObject
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }
    }

    [JsonObject]
    public abstract class DirectMessageEventObjectBase
    {
        [JsonProperty("created_timestamp"), JsonConverter(typeof(TimestampConverter))]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("target")]
        public RecipientIdObject Target { get; set; }

        [JsonProperty("sender_id")]
        public string SenderId { get; set; }
    }

    public class DirectMessageEvent : UserToUserEvent
    {
        public App App { get; set; }
        public MessageData Content { get; set; }
    }

    public class App
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }
    }

    [JsonObject]
    public class DirectMessageEventObject
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("created_timestamp"), JsonConverter(typeof(TimestampConverter))]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("message_create")]
        public MessageObject MessageCreate { get; set; }
    }

    [JsonObject]
    public class MessageObject : DirectMessageEventObjectBase
    {
        [JsonProperty("source_app_id")]
        public string SourceAppId { get; set; }

        [JsonProperty("message_data")]
        public MessageData MessageData { get; set; }
    }

    [JsonObject]
    public class RecipientIdObject
    {
        [JsonProperty("recipient_id")]
        public string RecipientId { get; set; }
    }

    public class DirectMessageIndicateTypingEvent : UserToUserEvent
    {
    }

    [JsonObject]
    public class DirectMessageIndicateTypingEventObject : DirectMessageEventObjectBase
    {
    }

    public class DirectMessageMarkReadEvent : UserToUserEvent
    {
        public long LastReadEventId { get; set; }
    }

    [JsonObject]
    public class DirectMessageMarkReadEventObject : DirectMessageEventObjectBase
    {
        [JsonProperty("last_read_event_id")]
        public string LastReadEventId { get; set; }
    }

    public class TweetDeleteEvent : Event, ITargetEvent<long>, ISourceEvent<long>
    {
        public long Target { get; set; }
        public long Source { get; set; }
    }

    [JsonObject]
    public class TweetDeleteEventObject
    {
        [JsonProperty("status")]
        public StatusIdsObject Status { get; set; }

        [JsonProperty("timestamp_ms"), JsonConverter(typeof(TimestampConverter))]
        public DateTimeOffset Timestamp { get; set; }
    }

    [JsonObject]
    public class StatusIdsObject
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }
    }

    [JsonObject]
    public class AppObject
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    [JsonObject]
    public class UserObject
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("created_timestamp"), JsonConverter(typeof(TimestampConverter))]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("screen_name")]
        public string ScreenName { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("protected")]
        public bool IsProtected { get; set; }

        [JsonProperty("verified")]
        public bool IsVerified { get; set; }

        [JsonProperty("followers_count")]
        public int FollowersCount { get; set; }

        [JsonProperty("friends_count")]
        public int FriendsCount { get; set; }

        [JsonProperty("statuses_count")]
        public int StatusesCount { get; set; }

        [JsonProperty("profile_image_url")]
        public string ProfileImageUrl { get; set; }

        [JsonProperty("profile_image_url_https")]
        public string ProfileImageUrlHttps { get; set; }
    }
}
