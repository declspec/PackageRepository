using Dapper;
using Fiksu.Database;
using PackageRepository.Database.Entities;
using PackageRepository.Enums;
using PackageRepository.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageRepository.Database.Repositories {
    public interface IPermissionRepository {
        Task<ThingPermissions> GetThingPermissionsAsync(ThingIdentifier identifier);
    }

    public class PermissionRepository : IPermissionRepository {
        private const string PermissionTypeOrganisation = "organisation";
        private const string PermissionTypeTeam = "team";
        private const string PermissionTypeUser = "user";

        private static readonly string SelectThingPermissionsQuery = GetSelectThingPermissionsQuery();

        private readonly IDbConnectionProvider _connectionProvider;

        public PermissionRepository(IDbConnectionProvider connectionProvider) {
            _connectionProvider = connectionProvider;
        }

        public async Task<ThingPermissions> GetThingPermissionsAsync(ThingIdentifier identifier) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false)) {
                var entities = await connection.QueryAsync<ThingPermissionEntity>(SelectThingPermissionsQuery, identifier).ConfigureAwait(false);

                var permissions = new ThingPermissions() {
                    Organisations = new Dictionary<long, Permissions>(),
                    Teams = new Dictionary<long, Permissions>(),
                    Users = new Dictionary<long, Permissions>()
                };

                foreach(var entity in entities) {
                    switch(entity.PermissionType) {
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

        private static string GetSelectThingPermissionsQuery() {
            return $@"SELECT op.id, op.permission_type, op.permissions
                FROM { Tables.Things } o
                INNER JOIN (
                    SELECT '{ PermissionTypeOrganisation }' AS permission_type, oop.organisation_id AS id, oop.permissions, oop.thing_id
                    FROM { Tables.ThingOrganisationPermissions } oop

                    UNION ALL

                    SELECT '{ PermissionTypeTeam }' AS permission_type, otp.team_id AS id, otp.permissions, otp.thing_id
                    FROM { Tables.ThingTeamPermissions } otp

                    UNION ALL

                    SELECT '{ PermissionTypeUser }' AS permission_type, oup.user_id AS id, oup.permissions, oup.thing_id
                    FROM { Tables.ThingUserPermissions } oup
                ) op ON op.thing_id = o.id
                WHERE o.organisation_id = @{nameof(ThingIdentifier.OrganisationId)}
                AND o.name = @{nameof(ThingIdentifier.Name)}
                AND o.type = @{nameof(ThingIdentifier.Type)}
            ";
        }
    }
}
