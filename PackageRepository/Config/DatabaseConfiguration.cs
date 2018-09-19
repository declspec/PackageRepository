using Dapper;
using Fiksu.Database;
using Fiksu.Database.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using PackageRepository.Database.Repositories;
using System.Reflection;

namespace PackageRepository.Config {
    public static class DatabaseConfiguration {
        // Create an extension method for the IServiceCollection
        public static void AddDatabase(this IServiceCollection services, string connectionString) {
            var provider = new SqliteConnectionProvider(connectionString);
            var assemblies = new[] {
                type
            };

            // Dapper config
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            // Run the migrations
            using (var connection = provider.GetConnectionAsync().Result) {
                var migrator = new SqliteDatabaseMigrator(connection, currentAssembly);
                migrator.MigrateToLatest();
            }

            // Add the service
            services.AddSingleton<IDbConnectionProvider>(provider);

            // Add repos
            services.AddSingleton<IThingRepository, ThingRepository>();
            services.AddSingleton<INpmPackageRepository, NpmPackageRepository>();
            services.AddSingleton<INpmTarballRepository, NpmTarballRepository>();
        }
    }
}
