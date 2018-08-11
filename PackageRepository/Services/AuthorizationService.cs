using PackageRepository.Database.Repositories;
using PackageRepository.Enums;
using PackageRepository.Models;
using System.Threading.Tasks;

namespace PackageRepository.Services {
    public interface IAuthorizationService {
        Task<bool> IsAuthorizedAsync(UserContext context, ThingIdentifier thingId, Permissions requiredPermissions);
    }

    public class AuthorizationService : IAuthorizationService {
        private readonly IPermissionRepository _permissionRepository;

        public AuthorizationService(IPermissionRepository permissionRepository) {
            _permissionRepository = permissionRepository;
        }

        public async Task<bool> IsAuthorizedAsync(UserContext context, ThingIdentifier thingId, Permissions requiredPermissions) {
            var thingPermissions = await _permissionRepository.GetThingPermissionsAsync(thingId).ConfigureAwait(false);
            var userPermissions = Permissions.None;

            if (thingPermissions.Users.TryGetValue(context.UserId, out var up))
                userPermissions |= up;

            if (thingPermissions.Organisations.TryGetValue(context.OrganisationId, out var op))
                userPermissions |= op;

            foreach (var team in context.Teams) {
                if (thingPermissions.Teams.TryGetValue(team, out var tp))
                    userPermissions |= tp;
            }

            return (userPermissions & requiredPermissions) == requiredPermissions;
        }
    }
}
