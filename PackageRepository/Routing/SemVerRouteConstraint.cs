using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Text.RegularExpressions;

namespace PackageRepository.Routing {
    public class SemVerRouteConstraint : IRouteConstraint {
        private static readonly Regex SemVerPattern = new Regex(@"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(-(0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(\.(0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*)?(\+[0-9a-zA-Z-]+(\.[0-9a-zA-Z-]+)*)?$", RegexOptions.Compiled);

        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection) {
            return values.TryGetValue(routeKey, out var value) && SemVerPattern.IsMatch(Convert.ToString(value));
        }
    }
}
