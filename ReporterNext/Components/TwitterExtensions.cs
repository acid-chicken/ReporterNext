using System;
using CoreTweet;
using Microsoft.Extensions.DependencyInjection;

namespace ReporterNext.Components
{
    public static class TwitterExtensions
    {
        public const long EpochTime = 1288834974657;

        public static DateTimeOffset ToSnowflake(this long snowflake) => DateTimeOffset.FromUnixTimeMilliseconds((snowflake >> 22) + EpochTime);
        public static DateTimeOffset ToSnowflake(this ulong snowflake) => ToSnowflake((long)snowflake);
        public static long ToSnowflakeLong(this DateTimeOffset dateTimeOffset) => (dateTimeOffset.ToUnixTimeMilliseconds() - EpochTime) << 22;
        public static ulong ToSnowflakeULong(this DateTimeOffset dateTimeOffset) => (ulong)dateTimeOffset.ToSnowflakeLong();

        public static IServiceCollection AddTwitter(this IServiceCollection services,
            string consumerKey,
            string consumerSecret,
            string accessToken,
            string accessTokenSecret,
            long accessTokenUserId,
            string accessTokenScreenName)
        {
            services.AddSingleton<Tokens>(Tokens.Create(consumerKey, consumerSecret, accessToken, accessTokenSecret, accessTokenUserId, accessTokenScreenName));
            services.AddSingleton<OAuth2Token>(OAuth2.GetTokenAsync(consumerKey, consumerSecret).GetAwaiter().GetResult());

            return services;
        }
    }
}
