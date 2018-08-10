using System.Data;
using Dapper;
using Fiksu.Database;

namespace PackageRepository.Database.Migrations {
    [Migration(201808101646)]
    public class CreateObjectTables : IMigration {
        public void Up(IDbConnection connection) {
            connection.Execute($@"CREATE TABLE { Tables.Objects } (
                id              INTEGER NOT NULL PRIMARY KEY,
                organisation_id INTEGER NOT NULL REFERENCES { Tables.Organisations } (id) ON DELETE CASCADE,
                type            INTEGER NOT NULL,
                name            TEXT NOT NULL,

                date_created    TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                date_modified   TIMESTAMP,

                CONSTRAINT unq_organisation_object_type_name UNIQUE(organisation_id, type, name)
            );");

            connection.Execute($@"CREATE TABLE { Tables.ObjectOrganisationPermissions } (
                object_id       INTEGER NOT NULL REFERENCES { Tables.Objects } (id) ON DELETE CASCADE,
                organisation_id INTEGER NOT NULL REFERENCES { Tables.Organisations } (id) ON DELETE CASCADE,
                permissions     INTEGER NOT NULL,

                date_created    TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                date_modified   TIMESTAMP
            );");

            connection.Execute($@"CREATE TABLE { Tables.ObjectTeamPermissions } (
                object_id       INTEGER NOT NULL REFERENCES { Tables.Objects } (id) ON DELETE CASCADE,
                team_id INTEGER NOT NULL REFERENCES { Tables.OrganisationTeams } (id) ON DELETE CASCADE,
                permissions     INTEGER NOT NULL,

                date_created    TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                date_modified   TIMESTAMP
            );");

            connection.Execute($@"CREATE TABLE { Tables.ObjectUserPermissions } (
                object_id       INTEGER NOT NULL REFERENCES { Tables.Objects } (id) ON DELETE CASCADE,
                user_id         INTEGER NOT NULL REFERENCES { Tables.OrganisationUsers } (id) ON DELETE CASCADE,
                permissions     INTEGER NOT NULL,

                date_created    TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                date_modified   TIMESTAMP
            );");

            connection.Execute($"CREATE INDEX idx_object_organisation_permissions ON { Tables.ObjectOrganisationPermissions } (object_id, organisation_id);");
            connection.Execute($"CREATE INDEX idx_object_team_permissions ON { Tables.ObjectTeamPermissions } (object_id, team_id);");
            connection.Execute($"CREATE INDEX idx_object_user_permissions ON { Tables.ObjectUserPermissions } (object_id, user_id);");
            connection.Execute($"CREATE INDEX idx_object_org_type ON { Tables.Objects } (organisation_id, type);");
        }

        public void Down(IDbConnection connection) {
            connection.Execute($"DROP TABLE { Tables.ObjectUserPermissions };");
            connection.Execute($"DROP TABLE { Tables.ObjectTeamPermissions };");
            connection.Execute($"DROP TABLE { Tables.ObjectOrganisationPermissions };");
            connection.Execute($"DROP TABLE { Tables.Objects };");
        }
    }
}
