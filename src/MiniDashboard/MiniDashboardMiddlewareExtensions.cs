using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using MiniDashboard.Cfg;

namespace MiniDashboard
{
    public static class MiniDashboardMiddlewareExtensions
    {
        public static IEndpointConventionBuilder MapMiniDashboard(this IEndpointRouteBuilder endpoints, Action<MiniDashboardConfiguration> setConfiguration = null)
        {
            var cfg = new MiniDashboardConfiguration();
            setConfiguration?.Invoke(cfg);

            var pipeline = endpoints.CreateApplicationBuilder()
                .UseMiddleware<MiniDashboardMiddleware>(cfg)
                .Build();

            return endpoints.MapGet($"{cfg.Route}/{{path?}}/{{id?}}/{{elemId?}}", pipeline);
        }
    }
}