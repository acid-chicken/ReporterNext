using System;
using CoreTweet;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ReporterNext.Models;

namespace ReporterNext.Components
{
    public class ReplyQuotedTimeObserver : IObserver<TweetCreateEvent>
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

        public void OnNext(TweetCreateEvent value)
        {
            var nullableId = value.Target.QuotedStatusId ?? value.Target.QuotedStatus?.Id;
            if (nullableId is long id)
            {
                BackgroundJob.Enqueue(() => _tokens.Statuses.UpdateAsync(
                    status => $"ツイート時刻：{id.ToSnowflake():HH:mm:ss.fff}",
                    in_reply_to_status_id => id,
                    auto_populate_reply_metadata => true,
                    tweet_mode => TweetMode.Extended));
            }
        }
    }

    public static partial class ServiceCollectionServiceExtensions
    {
        public static IServiceCollection AddReactiveInterface(this IServiceCollection services)
        {
            services.AddSingleton(new EventObservableFactory());

            return services;
        }


        public static IApplicationBuilder UseReactiveInterface(this IApplicationBuilder app, long forUserId = default)
        {
            var tokens = app.ApplicationServices.GetService<Tokens>();
            var factory = app.ApplicationServices.GetService<EventObservableFactory>();
            var replyObserver = new ReplyQuotedTimeObserver(forUserId, tokens);
            factory.Create<TweetCreateEvent>(forUserId)
                .Subscribe(replyObserver);

            return app;
        }
    }
}
