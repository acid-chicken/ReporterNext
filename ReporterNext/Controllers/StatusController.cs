using System;
using System.Linq;
using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace ReporterNext.Controllers
{
  public class StatusController : Controller
    {
        public IActionResult Index()
        {
            var oneDayAgo = DateTime.UtcNow - TimeSpan.FromDays(1);
            var threeHoursAgo = DateTime.UtcNow - TimeSpan.FromHours(3);
            var monitor = JobStorage.Current.GetMonitoringApi();
            var succeededs = monitor.HourlySucceededJobs()
                .Where(x => x.Key > oneDayAgo)
                .Select(x => x.Value)
                .Sum();
            var faileds = monitor.HourlyFailedJobs()
                .Where(x => x.Key > threeHoursAgo)
                .Select(x => x.Value)
                .Sum();
            var silent = succeededs == 0;
            var noFail = faileds == 0;

            ViewData["Status"] = noFail && !silent ? "ok" : "partial";
            ViewData["Description"] = noFail
                ? null
                : silent
                    ? "システムは稼働中で、かつ直近 24 時間で失敗したジョブもありませんが、成功したジョブもありません。システムが Twitter API と切り離されてしまっているか、アカウント凍結等に陥っている可能性があります。問題があれば @acid_chicken までご報告下さい。"
                    : $"システムは稼働中ですが、直近 3 時間で失敗したジョブが {faileds} 件あります。ツイート規制やアカウント凍結等に陥っている可能性があります。";

            return View();
        }
    }
}
