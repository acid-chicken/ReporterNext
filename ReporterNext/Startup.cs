using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            long.TryParse(Configuration["Twitter:AccessToken"].Split('-').FirstOrDefault(), out var result) ? result : default;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var accessTokenUserId = GetAccessTokenUserId();
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<ConnectionMultiplexer>(Redis);
            services.AddSingleton<CRC>(new CRC(KeyedHashAlgorithm.Create("HMACSHA256"), Configuration["Twitter:ConsumerSecret"]));
            services.AddHangfire(configuration =>
                configuration.UseRedisStorage(Redis));
            services.AddReactiveInterface();
            services.AddTwitter(Configuration["Twitter:ConsumerKey"],
                    Configuration["Twitter:ConsumerSecret"],
                    Configuration["Twitter:AccessToken"],
                    Configuration["Twitter:AccessTokenSecret"],
                    accessTokenUserId,
                    Configuration["Twitter:ScreenName"]);
            services.AddDirectoryBrowser();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            _ = env.IsDevelopment() ?
                app.UseDeveloperExceptionPage() :
                app.UseHsts();

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
            app.UseMvc(routes =>
                routes.MapRoute("default", "{controller=Status}/{action=Index}"));
        }
    }
}
