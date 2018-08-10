using System.Data;
using Dapper;
using Fiksu.Database;

namespace PackageRepository.Database.Migrations {
    [Migration(201808101527)]
    public class CreateOrganisationTables : IMigration {
        public void Up(IDbConnection connection) {
            connection.Execute($@"CREATE TABLE { Tables.Organisations } (
                id              INTEGER NOT NULL PRIMARY KEY,
                name            TEXT NOT NULL,

                date_created    TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                date_modified   TIMESTAMP,

                CONSTRAINT unq_organisation_name UNIQUE(name)
            );");

            connection.Execute($@"CREATE TABLE { Tables.OrganisationUsers } (
                id              INTEGER NOT NULL PRIMARY KEY,
                organisation_id INTEGER NOT NULL REFERENCES { Tables.Organisations } (id) ON DELETE CASCADE,
                username        TEXT NOT NULL,

                date_created    TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                date_modified   TIMESTAMP,

                CONSTRAINT unq_organisation_username UNIQUE(organisation_id, username)
            );");

            connection.Execute($@"CREATE TABLE { Tables.OrganisationTeams } (
                id              INTEGER NOT NULL PRIMARY KEY,
                organisation_id INTEGER NOT NULL REFERENCES { Tables.Organisations } (id) ON DELETE CASCADE,
                name            TEXT NOT NULL,
                normalized_name TEXT NOT NULL,

                date_created    TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                date_modified   TIMESTAMP,

                CONSTRAINT unq_organisation_team_name UNIQUE(organisation_id, normalized_name)
            );");

            connection.Execute($@"CREATE TABLE { Tables.OrganisationTeamMembers } (
                team_id         INTEGER NOT NULL REFERENCES { Tables.OrganisationTeams } (id) ON DELETE CASCADE,
                user_id         INTEGER NOT NULL REFERENCES { Tables.OrganisationUsers } (id) ON DELETE CASCADE,
                date_created    TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,

                PRIMARY KEY(team_id, user_id)
            ) WITHOUT ROWID;");
        }

        public void Down(IDbConnection connection) {
            connection.Execute($"DROP TABLE {Tables.OrganisationTeamMembers};");
            connection.Execute($"DROP TABLE {Tables.OrganisationTeams};");
            connection.Execute($"DROP TABLE {Tables.OrganisationUsers};");
            connection.Execute($"DROP TABLE {Tables.Organisations};");
        }
    }
}
