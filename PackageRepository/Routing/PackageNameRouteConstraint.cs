using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Text.RegularExpressions;

namespace PackageRepository.Routing {
    public class PackageNameRouteConstraint : IRouteConstraint {
        private static readonly Regex PackageNamePattern = new Regex("^[a-z0-9!()*-][a-z0-9!()*-._]{0,213}$", RegexOptions.Compiled);

        public virtual bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection) {
            return values.TryGetValue(routeKey, out var value) && PackageNamePattern.IsMatch(Convert.ToString(value).Replace("%2f", "/", StringComparison.OrdinalIgnoreCase));
        }
    }
}
