using Dapper;
using Fiksu.Database;
using PackageRepository.Enums;
using PackageRepository.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PackageRepository.Database.Repositories {
    public interface IThingRepository {
        Task<Thing> GetThingAsync(ThingIdentifier identifier);
        Task<Permissions> GetThingPermissionsForUserAsync(long thingId, UserContext user);
    }

    public class ThingRepository : IThingRepository {
        private const string PermissionTypeOrganisation = "organisation";
        private const string PermissionTypeTeam = "team";
        private const string PermissionTypeUser = "user";

        private static readonly string SelectThingQuery = GetSelectThingQuery();
        private static readonly string SelectThingPermissionsQuery = GetSelectThingPermissionsQuery();

        private readonly IDbConnectionProvider _connectionProvider;

        public ThingRepository(IDbConnectionProvider connectionProvider) {
            _connectionProvider = connectionProvider;
        }

        public async Task<Thing> GetThingAsync(ThingIdentifier identifier) {
            using(var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false)) {
                return await connection.QuerySingleOrDefaultAsync<Thing>(SelectThingQuery, identifier).ConfigureAwait(false);
            }
        }

        public async Task<Permissions> GetThingPermissionsForUserAsync(long thingId, UserContext user) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false)) {
                var param = new { ThingId = thingId, user.Teams, user.OrganisationId, user.UserId };
                var permissions = await connection.QueryAsync<Permissions>(SelectThingPermissionsQuery, param).ConfigureAwait(false);

                return permissions.Aggregate(Permissions.None, (total, p) => total | p); 
            }
        }

        private static string GetSelectThingQuery() {
            return $@"SELECT t.id, t.organisation_id, t.type, t.name FROM { Tables.Things } t
                INNER JOIN { Tables.Organisations } o ON o.id = t.organisation_id
                WHERE o.name = @Organisation
                AND t.type = @Type
                AND t.name = @Name";
        }

        private static string GetSelectThingPermissionsQuery() {
            return $@"SELECT permissions FROM (
                SELECT oop.thing_id, oop.permissions
                FROM { Tables.ThingOrganisationPermissions } oop
                WHERE oop.organisation_id = @{nameof(UserContext.OrganisationId)}

                UNION

                SELECT otp.thing_id, otp.permissions
                FROM { Tables.ThingTeamPermissions } otp
                WHERE otp.team_id IN (@{nameof(UserContext.Teams)})

                UNION

                SELECT oup.thing_id, oup.permissions
                FROM { Tables.ThingUserPermissions } oup
                WHERE oup.user_id = @{nameof(UserContext.UserId)}
            )
            WHERE thing_id = @ThingId";
        }
    }
}
