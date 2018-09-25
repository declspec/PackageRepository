using Dapper;
using Fiksu.Database;
using Fiksu.Database.Sqlite;
using GetPkg.Npm;
using GetPkg.Npm.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace PackageRepository.Config {
    public static class DatabaseConfiguration {
        // Create an extension method for the IServiceCollection
        public static void AddDatabase(this IServiceCollection services, string connectionString) {
            var provider = new SqliteConnectionProvider(connectionString);

            // Dapper config
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            // Run the migrations
            using (var connection = provider.GetConnectionAsync().Result) {
                var migrator = new SqliteDatabaseMigrator(connection, typeof(SqlitePackageRepository).Assembly);
                migrator.MigrateToLatest();
            }

            // Add the service
            services.AddSingleton<IDbConnectionProvider>(provider);

            // Add repos
            //services.AddSingleton<IThingRepository, ThingRepository>();
            services.AddSingleton<INpmPackageRepository, SqlitePackageRepository>();
            services.AddSingleton<INpmTarballRepository, SqliteTarballRepository>();
        }
    }
}
