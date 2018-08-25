using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ReporterNext.Components;

namespace ReporterNext
{
    public class Program
    {
        public static void Main(string[] args)
        {
            #region CultureInfo
            {
                CultureInfo.CurrentCulture =
                CultureInfo.CurrentUICulture =
                Thread.CurrentThread.CurrentCulture =
                Thread.CurrentThread.CurrentUICulture =
                CultureInfo.DefaultThreadCurrentCulture =
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
            }
            #endregion

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
