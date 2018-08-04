using Dapper;
using Fiksu.Database;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PackageRepository.Constants;
using PackageRepository.Database.Entities;
using PackageRepository.Errors;
using PackageRepository.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PackageRepository.Database.Repositories {
    public interface IPackageRepository {
        Task PublishPackageVersionsAsync(IEnumerable<PublishedPackageVersion> versions);
        Task UnpublishPackageVersionsAsync(IEnumerable<PackageIdentifier> identifiers);
        Task UpdatePackageVersionsAsync(IEnumerable<PackageVersion> versions);
        Task SetDistTagsAsync(string package, IDictionary<string, string> distTags);

        Task<Package> GetPackageAsync(string package);
        Task<Tarball> GetTarballAsync(PackageIdentifier identifier);
        Task<PackageVersion> GetPackageVersionAsync(PackageIdentifier identifier);
    }

    public class PackageRepository : IPackageRepository {
        private const int ErrorSqliteConstraint = 19;
        private const int ErrorSqliteConstraintUnique = 2067;

        private static readonly string CreateDistTagQuery = GetCreateDistTagQuery();
        private static readonly string CreatePackageVersionQuery = GetCreatePackageVersionQuery();
        private static readonly string CreateTarballQuery = GetCreateTarballQuery();
        private static readonly string SelectTarballQuery = GetSelectTarballQuery();
        private static readonly string UpdatePackageVersionQuery = GetUpdatePackageVersionQuery();

        private static readonly JsonSerializerSettings DefaultSerializerSettings = new JsonSerializerSettings() {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver() {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };

        private readonly IDbConnectionProvider _connectionProvider;

        public PackageRepository(IDbConnectionProvider connectionProvider) {
            _connectionProvider = connectionProvider;
        }

        public async Task PublishPackageVersionsAsync(IEnumerable<PublishedPackageVersion> versions) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false))
            using (var transaction = connection.BeginTransaction()) {
                try {
                    var tasks = versions.SelectMany(pkg => new Task[] {
                        connection.ExecuteAsync(CreatePackageVersionQuery, ToEntity(pkg.Version), transaction),
                        connection.ExecuteAsync(CreateTarballQuery, pkg.Tarball, transaction)
                    });

                    await Task.WhenAll(tasks).ConfigureAwait(false);
                    transaction.Commit();
                }
                catch (SqliteException ex) {
                    if (ex.SqliteErrorCode != ErrorSqliteConstraint || ex.SqliteExtendedErrorCode != ErrorSqliteConstraintUnique)
                        throw;
                    // Best place for this as any other check (i.e. a pre-emptive SELECT) could potentially race.
                    throw new PackageException(ErrorCodes.VersionConflict, ex);
                }
            }
        }

        public Task UpdatePackageVersionsAsync(IEnumerable<PackageVersion> packages) {
            return UpdatePackageVersionsAsync(packages.Select(ToEntity));
        }

        public Task UnpublishPackageVersionsAsync(IEnumerable<PackageIdentifier> identifiers) {
            return UpdatePackageVersionsAsync(identifiers.Select(id =>  new PackageVersionEntity() {
                Package = id.Name,
                Version = id.Version,
                Published = false,
                Manifest = null // This is COALESCED in the UPDATE so it won't actually clobber the manifest
            }));
        }

        public async Task SetDistTagsAsync(string package, IDictionary<string, string> distTags) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false)) {
                await Task.WhenAll(distTags.Select(kvp => {
                    var tag = new DistTagEntity() { Package = package, Tag = kvp.Key, Version = kvp.Value };
                    return connection.ExecuteAsync(CreateDistTagQuery, tag);
                }));
            }
        }

        public async Task<Package> GetPackageAsync(string package) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false))
            using (var transaction = connection.BeginTransaction()) {
                var param = new { Package = package };

                var versionsTask = GetVersionsWhere("package = @Package AND published = 1", param, connection, transaction);
                var distTagsTask = GetDistTagsWhere("package = @Package", param, connection, transaction);

                await Task.WhenAll(versionsTask, distTagsTask).ConfigureAwait(false);

                var versions = versionsTask.Result.Select(ToModel).ToList();

                if (versions.Count == 0)
                    return null;

                return new Package() {
                    Name = package,
                    Versions = versions,
                    DistTags = distTagsTask.Result.ToDictionary(r => r.Tag, r => r.Version)
                };
            }
        }

        public async Task<PackageVersion> GetPackageVersionAsync(PackageIdentifier identifier) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false)) {
                var versions = await GetVersionsWhere("package = @Name AND version = @Version AND published = 1", identifier, connection);
                return ToModel(versions.SingleOrDefault());
            }
        }

        public async Task<Tarball> GetTarballAsync(PackageIdentifier identifier) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false)) {
                var entity = await connection.QuerySingleAsync<TarballEntity>(SelectTarballQuery, identifier);
                return ToModel(entity);
            }
        }

        private async Task UpdatePackageVersionsAsync(IEnumerable<PackageVersionEntity> entities) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false))
            using (var transaction = connection.BeginTransaction()) {
                var tasks = entities.Select(pkg => connection.ExecuteAsync(UpdatePackageVersionQuery, pkg, transaction));
                await Task.WhenAll(tasks).ConfigureAwait(false);
                transaction.Commit();
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
                @{nameof(TarballEntity.Package)},
                @{nameof(TarballEntity.Version)},
                @{nameof(TarballEntity.Data)}
            );";
        }

        private static string GetUpdatePackageVersionQuery() {
            return $@"UPDATE { Tables.PackageVersions } SET 
                manifest = COALESCE(@{nameof(PackageVersionEntity.Manifest)}, manifest),
                published = @{nameof(PackageVersionEntity.Published)}, 
                date_modified = CURRENT_TIMESTAMP
                WHERE package = @{nameof(PackageVersionEntity.Package)} AND version = @{nameof(PackageVersionEntity.Version)}";
        }

        private static string GetSelectTarballQuery() {
            return $@"SELECT package, version, data FROM { Tables.PackageTarballs } 
                WHERE package = @{nameof(PackageIdentifier.Name)} AND version = @{nameof(PackageIdentifier.Version)}";
        }

        private static PackageVersion ToModel(PackageVersionEntity entity) {
            return entity == null ? null : new PackageVersion() {
                Id = new PackageIdentifier(entity.Package, entity.Version),
                Manifest = JsonConvert.DeserializeObject<Manifest>(entity.Manifest, DefaultSerializerSettings)
            };
        }

        private static Tarball ToModel(TarballEntity entity) {
            return entity == null ? null : new Tarball() {
                Package = new PackageIdentifier(entity.Package, entity.Version),
                Data = entity.Data
            };
        }

        private static PackageVersionEntity ToEntity(PackageVersion model) {
            return model == null ? null : new PackageVersionEntity() {
                Package = model.Id.Name,
                Version = model.Id.Version,
                Published = true,
                Manifest = JsonConvert.SerializeObject(model.Manifest, DefaultSerializerSettings)
            };
        }
        
        private static TarballEntity ToEntity(Tarball model) {
            return model == null ? null : new TarballEntity() {
                Package = model.Package.Name,
                Version = model.Package.Version,
                Data = model.Data
            };
        }
    }
}
