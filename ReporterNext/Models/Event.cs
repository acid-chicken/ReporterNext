using System;
using System.Collections.Generic;
using CoreTweet;
using CoreTweet.Core;
using Newtonsoft.Json;

namespace ReporterNext.Models
{
    [JsonObject]
    public class Event
    {
        [JsonProperty("for_user_id")]
        public string ForUserId { get; set; }

        [JsonProperty("tweet_create_events")]
        public Status[] TweetCreateEvents { get; set; }

        [JsonProperty("favorite_events")]
        public FavoriteEvent[] FavoriteEvents { get; set; }

        [JsonProperty("follow_events")]
        public SourceTargetEvent<User, User> FollowEvents { get; set; }

        [JsonProperty("block_events")]
        public SourceTargetEvent<User, User> BlockEvents { get; set; }

        [JsonProperty("mute_events")]
        public SourceTargetEvent<User, User> MuteEvents { get; set; }

        [JsonProperty("user_event")]
        public UserEvent UserEvent { get; set; }

        [JsonProperty("direct_message_events")]
        public DirectMessageEvent[] DirectMessageEvents { get; set; }

        [JsonProperty("direct_message_indicate_typing_events")]
        public DirectMessageIndicateTypingEvent[] DirectMessageIndicateTypingEvents { get; set; }

        [JsonProperty("direct_message_mark_read_events")]
        public DirectMessageMarkReadEvent[] DirectMessageMarkReadEvents { get; set; }

        [JsonProperty("tweet_delete_events")]
        public TweetDeleteEvent[] TweetDeleteEvents { get; set; }

        [JsonProperty("apps")]
        public IDictionary<string, AppObject> Apps { get; set; }

        [JsonProperty("users")]
        public IDictionary<string, UserObject> Users { get; set; }
    }

    [JsonObject]
    public class FavoriteEvent
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

    [JsonObject]
    public class SourceTargetEvent<TSource, TTarget>
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

    [JsonObject]
    public class UserEvent
    {
        [JsonProperty("revoke")]
        public UserRevokeEvent Revoke { get; set; }
    }

    [JsonObject]
    public class UserRevokeEvent
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
    public abstract class DirectMessageEventBase
    {
        [JsonProperty("created_timestamp"), JsonConverter(typeof(TimestampConverter))]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("target")]
        public RecipientIdObject Target { get; set; }

        [JsonProperty("sender_id")]
        public string SenderId { get; set; }
    }

    [JsonObject]
    public class DirectMessageEvent
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("created_timestamp"), JsonConverter(typeof(TimestampConverter))]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("message_create")]
        public Message MessageCreate { get; set; }
    }

    [JsonObject]
    public class Message : DirectMessageEventBase
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

    [JsonObject]
    public class MessageData
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("entities")]
        public Entities Entities { get; set; }

        [JsonProperty("quick_reply")]
        public QuickReply QuickReply { get; set; }

        [JsonProperty("ctas")]
        public CallToAction[] CallToActions { get; set; }
    }

    [JsonObject]
    public class QuickReply
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("options")]
        public QuickReplyOption[] Options { get; set; }
    }

    [JsonObject]
    public class QuickReplyOption
    {
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("metadata")]
        public string Metadata { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    [JsonObject]
    public class CallToAction
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("tco_url")]
        public string TcoUrl { get; set; }
    }

    [JsonObject]
    public class DirectMessageIndicateTypingEvent : DirectMessageEventBase
    {
    }

    [JsonObject]
    public class DirectMessageMarkReadEvent : DirectMessageEventBase
    {
        [JsonProperty("last_read_event_id")]
        public string LastReadEventId { get; set; }
    }

    [JsonObject]
    public class TweetDeleteEvent
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
