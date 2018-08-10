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
        Task<ObjectPermissions> GetObjectPermissionsAsync(ObjectIdentifier identifier);
    }

    public class PermissionRepository : IPermissionRepository {
        private const string PermissionTypeOrganisation = "organisation";
        private const string PermissionTypeTeam = "team";
        private const string PermissionTypeUser = "user";

        private static readonly string SelectObjectPermissionsQuery = GetSelectObjectPermissionsQuery();

        private readonly IDbConnectionProvider _connectionProvider;

        public PermissionRepository(IDbConnectionProvider connectionProvider) {
            _connectionProvider = connectionProvider;
        }

        public async Task<ObjectPermissions> GetObjectPermissionsAsync(ObjectIdentifier identifier) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false)) {
                var entities = await connection.QueryAsync<ObjectPermissionEntity>(SelectObjectPermissionsQuery, identifier).ConfigureAwait(false);

                var permissions = new ObjectPermissions() {
                    Organisations = new Dictionary<int, Permission>(),
                    Teams = new Dictionary<int, Permission>(),
                    Users = new Dictionary<int, Permission>()
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
                            throw new NotSupportedException($"Unexpected {nameof(ObjectPermissionEntity.PermissionType)} encountered: {entity.PermissionType}");
                    }
                }

                return permissions;
            }
        }

        private static string GetSelectObjectPermissionsQuery() {
            return $@"SELECT op.id, op.permission_type, op.permissions
                FROM { Tables.Objects } o
                INNER JOIN (
                    SELECT '{ PermissionTypeOrganisation }' AS permission_type, oop.organisation_id AS id, oop.permissions, oop.object_id
                    FROM { Tables.ObjectOrganisationPermissions } oop

                    UNION ALL

                    SELECT '{ PermissionTypeTeam }' AS permission_type, otp.team_id AS id, otp.permissions, otp.object_id
                    FROM { Tables.ObjectTeamPermissions } otp

                    UNION ALL

                    SELECT '{ PermissionTypeUser }' AS permission_type, oup.user_id AS id, oup.permissions, oup.object_id
                    FROM { Tables.ObjectUserPermissions } oup
                ) op ON op.object_id = o.id
                WHERE o.organisation_id = @{nameof(ObjectIdentifier.OrganisationId)}
                AND o.name = @{nameof(ObjectIdentifier.Name)}
                AND o.type = @{nameof(ObjectIdentifier.Type)}
            ";
        }
    }
}
