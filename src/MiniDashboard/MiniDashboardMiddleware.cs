using System.Collections.Generic;
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
        public static MiniDashboardConfiguration Cfg { get; private set; } // TODO: find better way to share cfg with UI middleware

        public MiniDashboardMiddleware(RequestDelegate next, MiniDashboardConfiguration miniDashboardConfiguration)
        {
            _next = next;
            Cfg = miniDashboardConfiguration;
            var actions = new Actions(miniDashboardConfiguration);
            Router
                .Add("/", actions.Get)
                .Add("/cards", actions.GetCards)
                .Add("/cards/{id}", actions.GetCardById)
                .Add("/charts", actions.GetCharts)
                .Add("/charts/{id}", actions.GetChartById)
                .Add("/tables", actions.GetTables)
                .Add("/tables/{id}", actions.GetTableById)
                .Add("/tables/{id}/{elemId}", actions.GetTableRowById);
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var responseBody = Router.MatchTemplate(httpContext.Request);
            var json = JsonConvert.SerializeObject(responseBody, Formatting.Indented, new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> {new StringEnumConverter()},
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsync(json);
        }
    }
}