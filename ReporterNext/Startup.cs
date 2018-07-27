using System;
using System.Collections.Generic;
using System.Linq;
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
                    new DashboardAuthorizationFilter(Configuration["Dashboard:Key"])
                }
            });

            app.UseMvc(routes =>
                routes.MapRoute("default", "{controller=Status}/{action=Index}"));
        }
    }
}
