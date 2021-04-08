using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MiniDashboard.Cfg;
using MiniDashboard.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace MiniDashboard
{
    public class MiniDashboardMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly Router Router = new();
        private readonly string _expectedAuthHeader;
        public static MiniDashboardConfiguration Cfg { get; private set; } // TODO: find better way to share cfg with UI middleware

        public MiniDashboardMiddleware(RequestDelegate next, MiniDashboardConfiguration miniDashCfg)
        {
            _next = next;
            Cfg = miniDashCfg;
            var actions = new Actions(miniDashCfg);
            Router
                .Add("/", actions.Get)
                .Add("/cards", actions.GetCards)
                .Add("/cards/{id}", actions.GetCardById)
                .Add("/charts", actions.GetCharts)
                .Add("/charts/{id}", actions.GetChartById)
                .Add("/tables", actions.GetTables)
                .Add("/tables/{id}", actions.GetTableById)
                .Add("/tables/{id}/{elemId}", actions.GetTableRowById);

            if (!string.IsNullOrEmpty(miniDashCfg.BasicAuthUserName) && !string.IsNullOrEmpty(miniDashCfg.BasicAuthPassword))
            {
                _expectedAuthHeader = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{miniDashCfg.BasicAuthUserName}:{miniDashCfg.BasicAuthPassword}"))}";
            }
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            // Auth
            if (_expectedAuthHeader != null && (!httpContext.Request.Headers.TryGetValue("Authorization", out var authHeader) || authHeader != _expectedAuthHeader))
            {
                httpContext.Response.Headers["WWW-Authenticate"] = "Basic";
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await httpContext.Response.WriteAsync("Unauthorized");
                return;
            }

            var responseBody = Router.MatchTemplate(httpContext.Request);
            var json = JsonConvert.SerializeObject(responseBody, Formatting.Indented, new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new StringEnumConverter() },
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsync(json);
        }
    }
}