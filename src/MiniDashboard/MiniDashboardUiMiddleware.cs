using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiniDashboard.Cfg;

namespace MiniDashboard
{
    public class MiniDashboardUiMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly MiniDashboardUiConfiguration _cfg;
        private readonly StaticFileMiddleware _staticFileMiddleware;
        private const string EmbeddedFileNamespace = "MiniDashboard.wwwroot.dist";

        public MiniDashboardUiMiddleware(RequestDelegate next, MiniDashboardUiConfiguration cfg, IWebHostEnvironment hostingEnvironment, ILoggerFactory loggerFactory)
        {
            _next = next;
            _cfg = cfg ?? new MiniDashboardUiConfiguration();
            _staticFileMiddleware = CreateStaticFileMiddleware(next, hostingEnvironment, loggerFactory);
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var httpMethod = httpContext.Request.Method;
            var path = httpContext.Request.Path.Value;
            var response = httpContext.Response;

            // Respond with redirect
            if (httpMethod == "GET" && path?.TrimEnd('/') == _cfg.Route)
            {
                var relativeRedirectPath = string.IsNullOrEmpty(path) || path.EndsWith("/")
                    ? "index.html"
                    : $"{path.Split('/').Last()}/index.html";

                response.StatusCode = 301;
                response.Headers["Location"] = relativeRedirectPath;
                return;
            }

            // Respond with html
            if (httpMethod == "GET" && path == $"{_cfg.Route}/index.html")
            {
                response.StatusCode = 200;
                response.ContentType = "text/html;charset=utf-8";

                await using var stream = typeof(MiniDashboardMiddleware).GetTypeInfo().Assembly.GetManifestResourceStream($"{EmbeddedFileNamespace}.index.html");
                var htmlBuilder = await new StreamReader(stream).ReadToEndAsync();

                htmlBuilder = htmlBuilder
                    .Replace("{{base_url}}", MiniDashboardMiddleware.Cfg.Route)
                    .Replace("{{login_url}}", MiniDashboardMiddleware.Cfg.LoginRoute)
                    .Replace("{{refresh_url}}", MiniDashboardMiddleware.Cfg.RefreshRoute);

                await response.WriteAsync(htmlBuilder, Encoding.UTF8);
                return;
            }

            await _staticFileMiddleware.Invoke(httpContext);
        }

        private StaticFileMiddleware CreateStaticFileMiddleware(RequestDelegate next, IWebHostEnvironment hostingEnvironment, ILoggerFactory loggerFactory)
        {
            var staticFileOptions = new StaticFileOptions
            {
                FileProvider = new EmbeddedFileProvider(typeof(MiniDashboardMiddleware).GetTypeInfo().Assembly, EmbeddedFileNamespace),
                RequestPath = _cfg.Route
            };

            return new StaticFileMiddleware(next, hostingEnvironment, Options.Create(staticFileOptions), loggerFactory);
        }
    }
}