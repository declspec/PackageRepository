using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PackageRepository.Auth;
using PackageRepository.Database.Repositories;
using PackageRepository.Enums;
using PackageRepository.Models;
using PackageRepository.Utils;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PackageRepository.Filters {
    public class NpmPackageAccessAttribute : TypeFilterAttribute {
        public NpmPackageAccessAttribute(string organisationParameter, string packageParameter, Permissions requiredPermissions) : base(typeof(NpmPackageAccessFilter)) {
            Arguments = new object[] { organisationParameter, packageParameter, requiredPermissions };
        }
    }

    public class NpmPackageAccessFilter : IAsyncActionFilter {
        private readonly IThingRepository _thingRepository;
        private readonly string _organisationParameter;
        private readonly string _packageParameter;
        private readonly Permissions _requiredPermissions;

        public NpmPackageAccessFilter(IThingRepository thingRepository, string organisationParameter, string packageParameter, Permissions requiredPermissions) {
            _thingRepository = thingRepository;
            _organisationParameter = organisationParameter;
            _packageParameter = packageParameter;
            _requiredPermissions = requiredPermissions;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
                context.Result = new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            else {
                var organisation = (string)context.RouteData.Values[_organisationParameter];
                var package = (string)context.RouteData.Values[_packageParameter];
                var identifier = new ThingIdentifier(organisation, ThingType.NpmPackage, PackageUtils.UnescapeName(package));

                var thing = await _thingRepository.GetThingAsync(identifier);

                if (thing == null)
                    context.Result = new StatusCodeResult((int)HttpStatusCode.NotFound);
                else {
                    var principal = context.HttpContext.User;

                    var user = new UserContext(
                        userId: long.Parse(principal.Identity.Name),
                        organisationId: long.Parse(principal.FindFirst(CustomClaimTypes.OrganisationId).Value),
                        teamIds: principal.FindAll(CustomClaimTypes.Teams).Select(c => long.Parse(c.Value)).ToList()
                    );

                    var permissions = await _thingRepository.GetThingPermissionsForUserAsync(thing.Id, user);
                    if ((permissions & _requiredPermissions) == _requiredPermissions)
                        await next.Invoke();
                    else {
                        context.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
                    }
                }
            }
        }
    }
}
