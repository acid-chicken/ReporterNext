using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ReporterNext.Components
{

    public interface INetworkRouteAuthorization
    {
        string Name { get; }
        string Value { get; }
    }

    public class NetworkRouteAuthorization : INetworkRouteAuthorization
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class NetworkRouteAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly INetworkRouteAuthorization _authorization;

        public NetworkRouteAuthorizationMiddleware(RequestDelegate next, INetworkRouteAuthorization authorization)
        {
            _next = next;
            _authorization = authorization;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(_authorization.Name, out var value) || !value.Any(x => x == _authorization.Value))
            {
                context.Response.StatusCode = 403;

                return;
            }

            await _next(context);
        }
    }

    public static class NetworkRouteAuthorizationExtensions
    {
        public static IServiceCollection AddNetworkRouteAuthorization(this IServiceCollection services, string name, string value)
        {
            services.AddSingleton<INetworkRouteAuthorization>(new NetworkRouteAuthorization()
            {
                Name = name,
                Value = value
            });

            return services;
        }

        public static IApplicationBuilder UseNetworkRouteAuthorization(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<NetworkRouteAuthorizationMiddleware>();

            return builder;
        }
    }
}
