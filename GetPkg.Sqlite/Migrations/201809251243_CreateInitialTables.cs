using Dapper;
using Fiksu.Database;
using System.Data;

namespace GetPkg.Sqlite.Migrations {
    [Migration(201809251243)]
    public class CreateInitialTables : IMigration {
        public void Up(IDbConnection connection) {
            connection.Execute($@"CREATE TABLE { Tables.Users } (
                id              TEXT NOT NULL PRIMARY KEY,
                email           TEXT NOT NULL,

                date_created    TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                date_modified   TIMESTAMP,

                CONSTRAINT unq_user_email UNIQUE(email)
            );");

            connection.Execute($@"CREATE TABLE { Tables.Tokens } (
                id              TEXT NOT NULL PRIMARY KEY,
                user_id         TEXT NULL FOREIGN KEY REFERENCES { Tables.Users }(id)

                date_created    TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                date_modified   TIMESTAMP
            );");
        }

        public void Down(IDbConnection connection) {
            connection.Execute($"DROP TABLE { Tables.Tokens };");
            connection.Execute($"DROP TABLE { Tables.Users };");
        }
    }
}
