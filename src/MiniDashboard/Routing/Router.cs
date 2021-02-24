using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using MiniDashboard.Data;
using MiniDashboard.Utils;

namespace MiniDashboard.Routing
{
    public class Router
    {
        private readonly Dictionary<string, Func<object>> _routes = new();
        private readonly Dictionary<string, Func<string, object>> _routesWithId = new();
        private readonly Dictionary<string, Func<string, string, object>> _routesWithElemId = new();

        public Router Add(string route, Func<object> action)
        {
            ValidateRoute(ref route);
            _routes[route] = action;
            return this;
        }

        public Router Add(string route, Func<string, object> action)
        {
            ValidateRoute(ref route);
            _routesWithId[route] = action;
            return this;
        }

        public Router Add(string route, Func<string, string, object> action)
        {
            ValidateRoute(ref route);
            _routesWithElemId[route] = action;
            return this;
        }

        public object MatchTemplate(HttpRequest request)
        {
            ThrowIf.IsFalse(_routes.TryGetValue("/", out var homeAction), "Home route is missing");

            var hasPath = request.RouteValues.TryGetValue("path", out var path) && path != null;
            var hasId = request.RouteValues.TryGetValue("id", out var id) && id != null;
            var hasElemId = request.RouteValues.TryGetValue("elemId", out var elemId) && elemId != null;

            if (hasElemId)
            {
                _routesWithElemId.TryGetValue(path as string, out var actionWithElemId);
                return actionWithElemId?.Invoke(id as string, elemId as string) ?? Error.NotFound();
            }

            if (hasId)
            {
                _routesWithId.TryGetValue(path as string, out var actionWithId);
                return actionWithId?.Invoke(id as string) ?? Error.NotFound();
            }

            if (hasPath)
            {
                _routes.TryGetValue(path as string, out var action);
                return action?.Invoke() ?? Error.NotFound();
            }

            return homeAction?.Invoke();
        }

        private static void ValidateRoute(ref string route)
        {
            ThrowIf.IsNullOrWhiteSpace(route);
            if (route == "/") return;

            route = $"{route.Trim('/')}";
            route = route.Split('/')[0];
        }
    }
}