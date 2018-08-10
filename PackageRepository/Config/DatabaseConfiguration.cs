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
            var currentAssembly = typeof(Startup).GetTypeInfo().Assembly;
            var provider = new SqliteConnectionProvider(connectionString);

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
            services.AddSingleton<IPackageRepository, Database.Repositories.PackageRepository>();
            services.AddSingleton<IPermissionRepository, PermissionRepository>();
            //services.AddSingleton<ITarballRepository, TarballRepository>();
        }
    }
}
