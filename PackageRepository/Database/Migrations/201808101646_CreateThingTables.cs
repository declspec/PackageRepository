using System.Data;
using Dapper;
using Fiksu.Database;

namespace PackageRepository.Database.Migrations {
    [Migration(201808101646)]
    public class CreateThingTables : IMigration {
        public void Up(IDbConnection connection) {
            connection.Execute($@"CREATE TABLE { Tables.Things } (
                id              INTEGER NOT NULL PRIMARY KEY,
                organisation_id INTEGER NOT NULL REFERENCES { Tables.Organisations } (id) ON DELETE CASCADE,
                type            INTEGER NOT NULL,
                name            TEXT NOT NULL,

                date_created    TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                date_modified   TIMESTAMP,

                CONSTRAINT unq_organisation_thing_type_name UNIQUE(organisation_id, type, name)
            );");

            connection.Execute($@"CREATE TABLE { Tables.ThingOrganisationPermissions } (
                thing_id       INTEGER NOT NULL REFERENCES { Tables.Things } (id) ON DELETE CASCADE,
                organisation_id INTEGER NOT NULL REFERENCES { Tables.Organisations } (id) ON DELETE CASCADE,
                permissions     INTEGER NOT NULL,

                date_created    TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                date_modified   TIMESTAMP
            );");

            connection.Execute($@"CREATE TABLE { Tables.ThingTeamPermissions } (
                thing_id       INTEGER NOT NULL REFERENCES { Tables.Things } (id) ON DELETE CASCADE,
                team_id INTEGER NOT NULL REFERENCES { Tables.OrganisationTeams } (id) ON DELETE CASCADE,
                permissions     INTEGER NOT NULL,

                date_created    TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                date_modified   TIMESTAMP
            );");

            connection.Execute($@"CREATE TABLE { Tables.ThingUserPermissions } (
                thing_id       INTEGER NOT NULL REFERENCES { Tables.Things } (id) ON DELETE CASCADE,
                user_id         INTEGER NOT NULL REFERENCES { Tables.OrganisationUsers } (id) ON DELETE CASCADE,
                permissions     INTEGER NOT NULL,

                date_created    TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                date_modified   TIMESTAMP
            );");

            connection.Execute($"CREATE INDEX idx_thing_organisation_permissions ON { Tables.ThingOrganisationPermissions } (thing_id, organisation_id);");
            connection.Execute($"CREATE INDEX idx_thing_team_permissions ON { Tables.ThingTeamPermissions } (thing_id, team_id);");
            connection.Execute($"CREATE INDEX idx_thing_user_permissions ON { Tables.ThingUserPermissions } (thing_id, user_id);");
            connection.Execute($"CREATE INDEX idx_thing_org_type ON { Tables.Things } (organisation_id, type);");
        }

        public void Down(IDbConnection connection) {
            connection.Execute($"DROP TABLE { Tables.ThingUserPermissions };");
            connection.Execute($"DROP TABLE { Tables.ThingTeamPermissions };");
            connection.Execute($"DROP TABLE { Tables.ThingOrganisationPermissions };");
            connection.Execute($"DROP TABLE { Tables.Things };");
        }
    }
}
