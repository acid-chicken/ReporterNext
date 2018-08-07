using System;
using Microsoft.Extensions.DependencyInjection;
using ReporterNext.Models;

namespace ReporterNext.Components
{
    public class ReplyObserver : IObserver<TweetCreateEvent>
    {
        private long _forUserId;

        public ReplyObserver(long forUserId)
        {
            _forUserId = forUserId;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(TweetCreateEvent value)
        {
        }
    }

    public static partial class ServiceCollectionServiceExtensions
    {
        public static IServiceCollection AddReactiveInterface(this IServiceCollection services, long forUserId = default)
        {
            var factory = new EventObservableFactory();
            services.AddSingleton(factory);

            var replyObserver = new ReplyObserver(forUserId);
            services.AddSingleton(replyObserver);
            factory.Create<TweetCreateEvent>(forUserId)
                .Subscribe(replyObserver);

            return services;
        }
    }
}
