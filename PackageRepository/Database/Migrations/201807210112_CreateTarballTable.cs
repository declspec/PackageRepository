using Dapper;
using Fiksu.Database;
using System.Data;

namespace PackageRepository.Database.Migrations {
    [Migration(201807210112)]
    class CreateTarballTable : IMigration {
        public void Up(IDbConnection connection) {
            connection.Execute($@"CREATE TABLE { Tables.PackageTarballs } (
                package         TEXT NOT NULL,
                version         TEXT NOT NULL,
                data            BLOB NOT NULL,

                date_created    TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,

                CONSTRAINT unq_package_version UNIQUE(package, version)
            );");
        }

        public void Down(IDbConnection connection) {
            connection.Execute($"DROP TABLE { Tables.PackageTarballs };");
        }        
    }
}
