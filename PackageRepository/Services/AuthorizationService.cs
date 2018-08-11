using PackageRepository.Database.Repositories;
using PackageRepository.Enums;
using PackageRepository.Models;
using System.Threading.Tasks;

namespace PackageRepository.Services {
    public interface IAuthorizationService {
        Task<bool> IsAuthorizedAsync(UserContext context, ObjectIdentifier objectId, Permissions requiredPermissions);
    }

    public class AuthorizationService : IAuthorizationService {
        private readonly IPermissionRepository _permissionRepository;

        public AuthorizationService(IPermissionRepository permissionRepository) {
            _permissionRepository = permissionRepository;
        }

        public async Task<bool> IsAuthorizedAsync(UserContext context, ObjectIdentifier objectId, Permissions requiredPermissions) {
            var objectPermissions = await _permissionRepository.GetObjectPermissionsAsync(objectId).ConfigureAwait(false);
            var userPermissions = Permissions.None;

            if (objectPermissions.Users.TryGetValue(context.UserId, out var up))
                userPermissions |= up;

            if (objectPermissions.Organisations.TryGetValue(context.OrganisationId, out var op))
                userPermissions |= op;

            foreach (var team in context.Teams) {
                if (objectPermissions.Teams.TryGetValue(team, out var tp))
                    userPermissions |= tp;
            }

            return (userPermissions & requiredPermissions) == requiredPermissions;
        }
    }
}
