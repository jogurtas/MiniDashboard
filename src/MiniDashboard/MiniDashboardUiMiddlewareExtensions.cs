using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MiniDashboard.Cfg;

namespace MiniDashboard
{
    public static class MiniDashboardUiMiddlewareExtensions
    {
        public static IApplicationBuilder UseMiniDashboardUi(this IApplicationBuilder builder, Action<MiniDashboardUiConfiguration> setConfiguration = null)
        {
            var cfg = new MiniDashboardUiConfiguration();
            if (setConfiguration != null)
                setConfiguration(cfg);
            else
                cfg = builder.ApplicationServices.GetRequiredService<IOptions<MiniDashboardUiConfiguration>>().Value;

            return builder.UseMiddleware<MiniDashboardUiMiddleware>(cfg);
        }
    }
}