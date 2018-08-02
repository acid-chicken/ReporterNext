using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace ReporterNext.Components
{
    public class DashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context) => true;
    }
}
