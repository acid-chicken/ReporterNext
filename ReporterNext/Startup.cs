﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.LiteDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReporterNext.Components;
using ReporterNext.Models;

namespace ReporterNext
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<CRC>(new CRC(KeyedHashAlgorithm.Create("HMACSHA256"), Configuration["Twitter:ConsumerSecret"]));
            services.AddReactiveInterface(Configuration.GetValue(
                "Twitter:ForUserId",
                long.TryParse(Configuration["Twitter:AccessToken"].Split('-').FirstOrDefault(), out var result) ? result : default));
            services.AddHangfire(configuration =>
                configuration.UseLiteDbStorage());
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

            app.UseMvc(routes =>
                routes.MapRoute("default", "{controller=Status}/{action=Index}"));
        }
    }
}
