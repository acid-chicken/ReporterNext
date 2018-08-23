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
            if (value.Target.IsQuotedStatus ?? false)
            {
                var id = value.Target.QuotedStatusId;
                if (!(id is null))
                    BackgroundJob.Enqueue(() => _tokens.Statuses.UpdateAsync(
                        status => $"",
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
            var factory = new EventObservableFactory();
            services.AddSingleton(factory);

            return services;
        }


        public static IApplicationBuilder UseReactiveInterface(this IApplicationBuilder app, long forUserId = default, Tokens tokens = default)
        {
            if (tokens is null)
                tokens = app.ApplicationServices.GetService<Tokens>();

            var factory = app.ApplicationServices.GetService<EventObservableFactory>();
            var replyObserver = new ReplyQuotedTimeObserver(forUserId, tokens);
            factory.Create<TweetCreateEvent>(forUserId)
                .Subscribe(replyObserver);

            return app;
        }
    }
}
