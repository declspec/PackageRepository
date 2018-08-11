using Dapper;
using Fiksu.Database;
using PackageRepository.Database.Entities;
using PackageRepository.Enums;
using PackageRepository.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageRepository.Database.Repositories {
    public interface IThingRepository {
        Task<Thing> GetThingAsync(ThingIdentifier identifier);
        Task<ThingPermissions> GetThingPermissionsAsync(long thingId);
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

        public async Task<ThingPermissions> GetThingPermissionsAsync(long thingId) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false)) {
                var param = new { ThingId = thingId };
                var entities = await connection.QueryAsync<ThingPermissionEntity>(SelectThingPermissionsQuery, param).ConfigureAwait(false);

                var permissions = new ThingPermissions() {
                    Organisations = new Dictionary<long, Permissions>(),
                    Teams = new Dictionary<long, Permissions>(),
                    Users = new Dictionary<long, Permissions>()
                };

                foreach (var entity in entities) {
                    switch (entity.PermissionType) {
                        case PermissionTypeOrganisation:
                            permissions.Organisations.Add(entity.Id, entity.Permissions);
                            break;
                        case PermissionTypeTeam:
                            permissions.Teams.Add(entity.Id, entity.Permissions);
                            break;
                        case PermissionTypeUser:
                            permissions.Users.Add(entity.Id, entity.Permissions);
                            break;
                        default:
                            throw new NotSupportedException($"Unexpected {nameof(ThingPermissionEntity.PermissionType)} encountered: {entity.PermissionType}");
                    }
                }

                return permissions;
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
            return $@"SELECT * FROM (
                SELECT '{ PermissionTypeOrganisation }' AS permission_type, oop.organisation_id AS id, oop.permissions, oop.thing_id
                FROM { Tables.ThingOrganisationPermissions } oop

                UNION ALL

                SELECT '{ PermissionTypeTeam }' AS permission_type, otp.team_id AS id, otp.permissions, otp.thing_id
                FROM { Tables.ThingTeamPermissions } otp

                UNION ALL

                SELECT '{ PermissionTypeUser }' AS permission_type, oup.user_id AS id, oup.permissions, oup.thing_id
                FROM { Tables.ThingUserPermissions } oup
            )
            WHERE thing_id = @ThingId";
        }
    }
}
