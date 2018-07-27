using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace ReporterNext.Components
{
    public class DashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private string _ip = "";

        internal DashboardAuthorizationFilter(string ip) =>
            _ip = ip;

        public bool Authorize([NotNull] DashboardContext context) =>
            context.Request.RemoteIpAddress == _ip;
    }
}
