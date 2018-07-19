using System.Data;
using Dapper;
using Fiksu.Database;

namespace PackageRepository.Database.Migrations {
    [Migration(201807182200)]
    public class CreateBaseTables : IMigration {
        public void Up(IDbConnection connection) {
            connection.Execute($@"CREATE TABLE { Tables.PackageVersions } (
                name            TEXT NOT NULL,
                version         TEXT NOT NULL,
                manifest        TEXT NOT NULL,
                published       BOOLEAN NOT NULL DEFAULT 1,

                date_created    TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                date_modified   TIMESTAMP,

                CONSTRAINT unq_package_version UNIQUE(name, version)
            );");

            connection.Execute($@"CREATE TABLE { Tables.DistTags } (
                package         TEXT NOT NULL,
                tag             TEXT NOT NULL,
                version         TEXT NOT NULL,

                date_created    TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                date_modified   TIMESTAMP,

                CONSTRAINT unq_package_tag UNIQUE(package, tag)
            );");
        }

        public void Down(IDbConnection connection) {
            connection.Execute($"DROP TABLE { Tables.DistTags };");
            connection.Execute($"DROP TABLE { Tables.PackageVersions };");
        }
    }
}
