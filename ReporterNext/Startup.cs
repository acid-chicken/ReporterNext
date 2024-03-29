using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReporterNext.Components;
using ReporterNext.Models;
using StackExchange.Redis;

namespace ReporterNext
{
    public class Startup
    {
        private readonly static IEnumerable<string> _immutableExtensions = new []
        {
            ".otf",
            ".woff",
            ".woff2"
        };

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Redis = ConnectionMultiplexer.Connect(Configuration.GetConnectionString("Redis"));
        }

        public IConfiguration Configuration { get; }

        public ConnectionMultiplexer Redis { get; }

        private long GetAccessTokenUserId() =>
            long.TryParse(Configuration["Twitter:AccessToken"]?.Split('-').FirstOrDefault(), out var result) ? result : default;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var accessTokenUserId = GetAccessTokenUserId();
            services.AddSingleton(Configuration);
            services.AddSingleton(Redis);
            services.AddSingleton(new ConcurrentDictionary<long, long>());
            services.AddSingleton(new CRC(KeyedHashAlgorithm.Create("HMACSHA256"), Configuration["Twitter:ConsumerSecret"]));
            services.AddNetworkRouteAuthorization(Configuration["NetworkRouteAuthorization:Name"], Configuration["NetworkRouteAuthorization:Value"]);
            services.AddHangfire(configuration =>
                configuration.UseRedisStorage(Redis));
            services.AddReactiveInterface();
            services.AddInteractiveInterface();
            services.AddTwitter(Configuration["Twitter:ConsumerKey"],
                    Configuration["Twitter:ConsumerSecret"],
                    Configuration["Twitter:AccessToken"],
                    Configuration["Twitter:AccessTokenSecret"],
                    accessTokenUserId,
                    Configuration["Twitter:ScreenName"]);
            services.AddDirectoryBrowser();
            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });
            services.AddControllers()
                .AddNewtonsoftJson();
            services.AddRazorPages();
            //services.AddMvc()
                //.SetCompatibilityVersion(CompatibilityVersion.Latest);
                //.AddNewtonsoftJson(options =>
                //    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            _ = env.IsDevelopment() ?
                app.UseDeveloperExceptionPage() :
                app.UseHsts().UseNetworkRouteAuthorization();

            app.UseForwardedHeaders(new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.All
            });
            app.UseHttpsRedirection();

            app.UseHangfireServer();
            app.UseHangfireDashboard("/dashboard", new DashboardOptions()
            {
                Authorization = new []
                {
                    new DashboardAuthorizationFilter()
                }
            });
            app.UseReactiveInterface(Configuration.GetValue("Twitter:ForUserId", GetAccessTokenUserId()));
            app.UseInteractiveInterface();

            app.UseDefaultFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = x =>
                {
                    x.Context.Response.Headers.Append("Cache-Control", $"public, max-age={(env.IsDevelopment() ? "600" : "604800")}{(_immutableExtensions.Any(y => x.File.Name.EndsWith(y)) ? ", immutable" : "")}");
                }
            });

            app.UseFileServer(new FileServerOptions()
            {
                RequestPath = "/wwwroot",
                EnableDirectoryBrowsing = env.IsDevelopment()
            });
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Status}/{action=Index}");
                endpoints.MapRazorPages();
            });
        }
    }
}
