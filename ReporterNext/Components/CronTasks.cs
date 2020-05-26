using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CoreTweet;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ReporterNext.Components
{
    public class CronTasks
    {
        public const string PickOneFromUserTimeline = "pick_one_from_user_timeline";
        public const string PickOneFromUserTimelineInducer = "p";

        public static IDictionary<string, KeyValuePair<string, string>> AvailableTargets = new Dictionary<string, KeyValuePair<string, string>>
        {
            ["334"] = KeyValuePair.Create("334", "334"),
        };

        public static IDictionary<string, string> ObsoleteTargets = new Dictionary<string, string>
        {
        };

        public static async Task TweetQuickReplyInducerAsync(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, string key, string value)
        {
            var tokens = Tokens.Create(consumerKey, consumerSecret, accessToken, accessTokenSecret);
            var myId = long.Parse(accessToken.Split('-')[0]);
            var name = $"${PickOneFromUserTimelineInducer}#${key}";

            Task<WelcomeMessage> FindTargetedWelcomeMessage(string nextCursor = default) =>
                tokens.DirectMessages.WelcomeMessages.ListAsync(
                    count => 50,
                    cursor => nextCursor)
                    .ContinueWith(x => x.Result.Any() ?
                        Task.FromResult(x.Result.FirstOrDefault(x => x.Name == name)) ?? FindTargetedWelcomeMessage(x.Result.NextCursor) :
                        Task.FromResult(default(WelcomeMessage)))
                    .Unwrap();

            var welcomeMessage = await FindTargetedWelcomeMessage();

            if (welcomeMessage is null)
            {
                var response = await tokens.DirectMessages.WelcomeMessages.NewAsync(
                    text => $"こちらから最新の「{value}」ツイートの投稿時刻をミリ秒単位で照会できます。",
                    quick_reply => new QuickReply()
                    {
                        Type = "options",
                        Options = new []
                        {
                            new QuickReplyOption()
                            {
                                Label = $"「${value}」で照会",
                                Description = $"最新の「{value}」ツイートの投稿時刻をミリ秒単位で照会します。",
                                Metadata = $"${PickOneFromUserTimeline}:${key}",
                            },
                        },
                    },
                    name => name);

                welcomeMessage = response.WelcomeMessage;
            }

            await tokens.Statuses.UpdateAsync(
                status => $"こちらのリンクからダイレクトメッセージ経由で最新の「${value}」ツイートの投稿時刻をミリ秒単位で照会できます。リンクを使用せずに直接リプライあるいはダイレクトメッセージで対象ツイートを引用するか、ツイートスレッドでメンションすることでも照会可能です。 https://twitter.com/messages/compose?recipient_id=${myId}&weelcome_message_id=${welcomeMessage.Id}",
                auto_populate_reply_metadata => true,
                include_ext_alt_text => true,
                tweet_mode => TweetMode.Extended);
        }
    }

    public static partial class ServiceCollectionServiceExtensions
    {
        public static IServiceCollection AddInteractiveInterface(this IServiceCollection services)
        {
            foreach (var target in CronTasks.ObsoleteTargets.Keys)
                RecurringJob.RemoveIfExists(target);

            return services;
        }

        public static IApplicationBuilder UseInteractiveInterface(this IApplicationBuilder app)
        {
            var tokens = app.ApplicationServices.GetService<Tokens>();

            foreach (var target in CronTasks.AvailableTargets)
                RecurringJob.AddOrUpdate(target.Key, () => CronTasks.TweetQuickReplyInducerAsync(tokens.ConsumerKey, tokens.ConsumerSecret, tokens.AccessToken, tokens.AccessTokenSecret, target.Key, target.Value.Key), "33 18 * * *");

            return app;
        }
    }
}
