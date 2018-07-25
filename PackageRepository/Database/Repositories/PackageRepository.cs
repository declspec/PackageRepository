using Dapper;
using Fiksu.Database;
using Microsoft.Data.Sqlite;
using PackageRepository.Database.Entities;
using PackageRepository.Errors;
using PackageRepository.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PackageRepository.Database.Repositories {
    public interface IPackageRepository {
        Task CreatePublishedPackageAsync(PublishedPackage package);
        Task<PackageOverview> GetPackageOverviewAsync(string package);
        /*

        Task CreateVersionAsync(PublishedPackage package);
        Task<PublishedPackage> GetPackageVersionAsync(PackageIdentifier identifier);
        Task<IList<PackageVersion>> GetPackageVersionsAsync(string packageName);

        Task<IDictionary<string, string>> GetPackageDistTagsAsync(string packageName);

        Task<Tarball> GetPackageTarballAsync(PackageIdentifier identifier);*/
    }

    public class PackageRepository : IPackageRepository {
        private const int ErrorSqliteConstraint = 19;
        private const int ErrorSqliteConstraintUnique = 2067;

        private static readonly string CreateDistTagQuery = GetCreateDistTagQuery();
        private static readonly string CreatePackageVersionQuery = GetCreatePackageVersionQuery();
        private static readonly string CreateTarballQuery = GetCreateTarballQuery();
        private static readonly string SelectTarballQuery = GetSelectTarballQuery();

        private readonly IDbConnectionProvider _connectionProvider;

        public PackageRepository(IDbConnectionProvider connectionProvider) {
            _connectionProvider = connectionProvider;
        }

        public async Task CreatePublishedPackageAsync(PublishedPackage package) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false))
            using (var transaction = connection.BeginTransaction()) {
                // Batch all the of the queries
                var versionTask = connection.ExecuteAsync(CreatePackageVersionQuery, ToEntity(package.Version), transaction);
                var tarballTask = connection.ExecuteAsync(CreateTarballQuery, package.Tarball, transaction);

                var distTagTasks = package.DistTags?.Select(kvp => {
                    var tag = new DistTagEntity() { Package = package.Version.Id.Name, Tag = kvp.Key, Version = kvp.Value };
                    return connection.ExecuteAsync(CreateDistTagQuery, tag, transaction);
                }) ?? Enumerable.Empty<Task>();

                try {
                    await Task.WhenAll(distTagTasks.Concat(new[] { versionTask, tarballTask })).ConfigureAwait(false);
                    transaction.Commit();
                }
                catch (SqliteException ex) {
                    if (ex.SqliteErrorCode != ErrorSqliteConstraint || ex.SqliteExtendedErrorCode != ErrorSqliteConstraintUnique)
                        throw;
                    // Best place for this as any other check (i.e. a pre-emptive SELECT) could potentially race.
                    throw new DuplicatePackageVersionException(package.Version.Id, ex);
                }
            }
        }

        public async Task<PackageOverview> GetPackageOverviewAsync(string package) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false))
            using (var transaction = connection.BeginTransaction()) {
                var param = new { Package = package };

                var versionsTask = GetVersionsWhere("package = @Package", param, connection, transaction);
                var distTagsTask = GetDistTagsWhere("package = @Package", param, connection, transaction);

                await Task.WhenAll(versionsTask, distTagsTask).ConfigureAwait(false);

                var versions = versionsTask.Result.Select(ToModel).ToList();

                if (versions.Count == 0)
                    return null;

                return new PackageOverview() {
                    Name = package,
                    Versions = versions,
                    DistTags = distTagsTask.Result.ToDictionary(r => r.Tag, r => r.Version)
                };
            }
        }
        
        private Task<IEnumerable<PackageVersionEntity>> GetVersionsWhere(string clause, object param, IDbConnection connection, IDbTransaction transaction = null) {
            var query = $"SELECT package, version, manifest FROM { Tables.PackageVersions } WHERE { clause } ORDER BY version ASC";
            return connection.QueryAsync<PackageVersionEntity>(query, param, transaction);
        }

        private Task<IEnumerable<DistTagEntity>> GetDistTagsWhere(string clause, object param, IDbConnection connection, IDbTransaction transaction = null) {
            var query = $"SELECT tag, version FROM { Tables.DistTags } WHERE { clause }";
            return connection.QueryAsync<DistTagEntity>(query, param, transaction);
        }

        private static string GetCreateDistTagQuery() {
            return $@"INSERT OR REPLACE INTO { Tables.DistTags } (package, tag, version, date_created, date_modified) VALUES (
                @{nameof(DistTagEntity.Package)},
                @{nameof(DistTagEntity.Tag)},
                @{nameof(DistTagEntity.Version)},
                COALESCE((SELECT date_created FROM {Tables.DistTags} WHERE package = @{nameof(DistTagEntity.Package)} AND tag = @{nameof(DistTagEntity.Tag)}), CURRENT_TIMESTAMP),
                (SELECT CURRENT_TIMESTAMP FROM {Tables.DistTags} WHERE package = @{nameof(DistTagEntity.Package)} AND tag = @{nameof(DistTagEntity.Tag)})
            );";
        }
        
        private static string GetCreatePackageVersionQuery() {
            return $@"INSERT INTO { Tables.PackageVersions } (package, version, manifest) VALUES (
                @{nameof(PackageVersionEntity.Package)},
                @{nameof(PackageVersionEntity.Version)},
                @{nameof(PackageVersionEntity.Manifest)}
            );";
        }

        private static string GetCreateTarballQuery() {
            return $@"INSERT INTO { Tables.PackageTarballs } (package, version, data) VALUES (
                @{nameof(Tarball.Package.Name)},
                @{nameof(Tarball.Package.Version)},
                @{nameof(Tarball.Data)}
            );";
        }

        private static string GetSelectTarballQuery() {
            return $@"SELECT package, version, data FROM { Tables.PackageTarballs } 
                WHERE package = @{nameof(PackageIdentifier.Name)} AND version = @{nameof(PackageIdentifier.Version)}";
        }

        private static PackageVersion ToModel(PackageVersionEntity entity) {
            if (entity == null)
                return null;

            return new PackageVersion() {
                Id = new PackageIdentifier(entity.Package, entity.Version),
                Manifest = entity.Manifest
            };
        }

        private static PackageVersionEntity ToEntity(PackageVersion model) {
            if (model == null)
                return null;

            return new PackageVersionEntity() {
                Package = model.Id.Name,
                Version = model.Id.Version,
                Manifest = model.Manifest
            };
        }
    }
}
