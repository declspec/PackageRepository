using System.Data;
using Dapper;
using Fiksu.Database;

namespace PackageRepository.Database.Migrations {
    [Migration(201808180021)]
    public class CreateNpmPackageTables : IMigration {
        public void Up(IDbConnection connection) {
            connection.Execute($@"CREATE TABLE { Tables.NpmPackages } (
                organisation    TEXT NOT NULL,
                name            TEXT NOT NULL,
                revision        TEXT NOT NULL,
                package         BLOB NOT NULL,

                date_created    TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                date_modified   TIMESTAMP,

                CONSTRAINT unq_npm_package_name UNIQUE(organisation, name)
            );");

            connection.Execute($@"CREATE TABLE { Tables.NpmTarballs } (
                organisation    TEXT NOT NULL,
                package         TEXT NOT NULL,
                version         TEXT NOT NULL,
                data            BLOB NOT NULL,

                date_created    TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL

                CONSTRAINT unq_npm_package_tarball UNIQUE(organisation, package, version)
            );");
        }

        public void Down(IDbConnection connection) {
            connection.Execute($"DROP TABLE {Tables.NpmTarballs};");
            connection.Execute($"DROP TABLE {Tables.NpmPackages};");
        }
    }
}
