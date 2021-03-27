using System;
using System.Collections.Concurrent;
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
    public class CronTasks
    {
        public const string PickOneFromUserTimeline = "pick_one_from_user_timeline";
        public const string PickOneFromUserTimelineInducer = "p";

        public static IReadOnlyDictionary<string, Ranker> AvailableTargets = new Dictionary<string, Ranker>
        {
            ["334"] = new Ranker()
            {
                Title = "334",
                Rule = "^334$",
                Origin = new TimeSpan(18, 34, 00).Ticks,
            },
        };

        public static IReadOnlyCollection<string> ObsoleteTargets = new string[]
        {
        };

        public static ConcurrentDictionary<long, long> CurrentMetrics { get; set; }

        public static async Task TweetQuickReplyInducerAsync(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, string key, Ranker value)
        {
            var tokens = Tokens.Create(consumerKey, consumerSecret, accessToken, accessTokenSecret);
            var myId = long.Parse(accessToken.Split('-')[0]);
            var keyName = $"{PickOneFromUserTimelineInducer}#{key}";

            Task<WelcomeMessage> FindTargetedWelcomeMessageAsync(string nextCursor = default) =>
                tokens.DirectMessages.WelcomeMessages.ListAsync(
                    count => 50,
                    cursor => nextCursor)
                    .ContinueWith(x => x.Result.Any() ?
                        Task.FromResult(x.Result.FirstOrDefault(x => x.Name == keyName)) ?? FindTargetedWelcomeMessageAsync(x.Result.NextCursor) :
                        Task.FromResult(default(WelcomeMessage)))
                    .Unwrap();

            var welcomeMessage = await FindTargetedWelcomeMessageAsync();

            if (welcomeMessage is null)
            {
                var response = await tokens.DirectMessages.WelcomeMessages.NewAsync(
                    text => $"こちらから最新の「{value.Title}」ツイートの投稿時刻をミリ秒単位で照会できます。",
                    quick_reply => new QuickReply()
                    {
                        Type = "options",
                        Options = new []
                        {
                            new QuickReplyOption()
                            {
                                Label = $"「{value.Title}」で照会",
                                Description = $"最新の「{value.Title}」ツイートの投稿時刻をミリ秒単位で照会します。",
                                Metadata = $"{PickOneFromUserTimeline}:{key}",
                            },
                        },
                    },
                    name => keyName);

                welcomeMessage = response.WelcomeMessage;
            }

            Task<StatusResponse> AnnounceAsync() =>
                tokens.Statuses.UpdateAsync(
                    status => $"こちらのリンクからダイレクトメッセージ経由で最新の「{value.Title}」ツイートの投稿時刻をミリ秒単位で照会できます。リンクを使用せずに直接リプライあるいはダイレクトメッセージで対象ツイートを引用するか、ツイートスレッドでメンションすることでも照会可能です。 https://twitter.com/messages/compose?recipient_id={myId}&welcome_message_id={welcomeMessage.Id}",
                    auto_populate_reply_metadata => true,
                    include_ext_alt_text => true,
                    tweet_mode => TweetMode.Extended);

            var target = new DateTimeOffset(DateTimeOffset.UtcNow.Date, TimeSpan.Zero) + new TimeSpan(value.Origin);
            var divisor = default(double);
            var dividend = default(double);

            foreach (var item in CurrentMetrics.ToArray())
            {
                var time = item.Key.ToSnowflake();
                var days = 1 - (target - time).TotalDays;

                if (days > 0)
                {
                    var weight = 1 / Math.Max(1 - days, double.Epsilon);

                    divisor += weight;
                    dividend += weight * item.Value;
                }
                else
                {
                    CurrentMetrics.TryRemove(item.Key, out _);
                }
            }

            var wait = TimeSpan.FromMilliseconds(divisor > 0 ? dividend / divisor : 0);
            var delay = target - wait - DateTimeOffset.UtcNow;

            if (TimeSpan.FromDays(1) > delay && delay > TimeSpan.Zero)
            {
                await Task.Delay(delay);
            }

            await AnnounceAsync();
        }
    }

    public static partial class ServiceCollectionServiceExtensions
    {
        public static IServiceCollection AddInteractiveInterface(this IServiceCollection services)
        {
            foreach (var target in CronTasks.ObsoleteTargets)
                RecurringJob.RemoveIfExists(target);

            return services;
        }

        public static IApplicationBuilder UseInteractiveInterface(this IApplicationBuilder app)
        {
            var metrics = app.ApplicationServices.GetService<ConcurrentDictionary<long, long>>();
            var tokens = app.ApplicationServices.GetService<Tokens>();

            CronTasks.CurrentMetrics = metrics;

            foreach (var target in CronTasks.AvailableTargets)
                RecurringJob.AddOrUpdate(target.Key, () => CronTasks.TweetQuickReplyInducerAsync(tokens.ConsumerKey, tokens.ConsumerSecret, tokens.AccessToken, tokens.AccessTokenSecret, target.Key, target.Value), "33 18 * * *");

            return app;
        }
    }
}
