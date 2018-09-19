using Dapper;
using Fiksu.Database;
using GetPkg.Npm.Exceptions;
using GetPkg.Npm.Sqlite.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GetPkg.Npm.Sqlite {
    public class SqlitePackageRepository : INpmPackageRepository {
        private static readonly string SelectPackageQuery = GetSelectPackageQuery();
        private static readonly string UpdatePackageQuery = GetUpdatePackageQuery();
        private static readonly string InsertPackageQuery = GetInsertPackageQuery();

        private static readonly JsonSerializer Serializer = new JsonSerializer();

        private readonly IDbConnectionProvider _connectionProvider;

        public SqlitePackageRepository(IDbConnectionProvider connectionProvider) {
            _connectionProvider = connectionProvider;
        }

        public async Task<Package> GetAsync(PackageIdentifier identifier) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false)) {
                var entity = await connection.QuerySingleOrDefaultAsync<PackageEntity>(SelectPackageQuery, identifier).ConfigureAwait(false);
                return ToModel(entity);
            }
        }

        public async Task<string> CreateAsync(Package package) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false)) {
                var entity = ToEntity(package);
                await connection.ExecuteAsync(InsertPackageQuery, entity).ConfigureAwait(false);
                return entity.NextRevision;
            }
        }

        public async Task<string> UpdateAsync(Package package) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false)) {
                var entity = ToEntity(package);
                var updated = await connection.ExecuteAsync(UpdatePackageQuery, entity).ConfigureAwait(false);

                if (updated != 1)
                    throw new PackageConflictException(new PackageIdentifier(package.Organisation, package.Name));

                return entity.NextRevision;
            }
        }

        private static Package ToModel(PackageEntity entity) {
            if (entity == null)
                return null;

            using (var ms = new MemoryStream(entity.Package))
            using (var reader = new BsonDataReader(ms)) {
                var model = Serializer.Deserialize<Package>(reader);
                model.Revision = entity.Revision;

                return model;
            }
        }

        private static PackageEntity ToEntity(Package model) {
            return model == null ? null : new PackageEntity() {
                Name = model.Name,
                Organisation = model.Organisation,
                Revision = model.Revision,
                Package = SerializePackage(model),
                NextRevision = Guid.NewGuid().ToString()
            };
        }

        private static byte[] SerializePackage(Package package) {
            using (var ms = new MemoryStream())
            using (var writer = new BsonDataWriter(ms)) {
                Serializer.Serialize(writer, package);
                return ms.ToArray();
            }
        }

        private static string GetSelectPackageQuery() {
            return $"SELECT organisation, name, revision, package FROM { Tables.Packages } WHERE organisation = @Organisation AND name = @Name";
        }

        private static string GetInsertPackageQuery() {
            return $@"INSERT INTO { Tables.Packages } (organisation, name, package, revision) VALUES (
                @{nameof(PackageEntity.Organisation)},
                @{nameof(PackageEntity.Name)},
                @{nameof(PackageEntity.Package)},
                @{nameof(PackageEntity.NextRevision)}
            );";
        }

        private static string GetUpdatePackageQuery() {
            return $@"UPDATE {Tables.Packages} SET
                revision = @{nameof(PackageEntity.NextRevision)},
                package = @{nameof(PackageEntity.Package)},
                date_modified = CURRENT_TIMESTAMP
                WHERE organisation = @{nameof(PackageEntity.Organisation)}
                AND name = @{nameof(PackageEntity.Name)}
                AND revision = @{nameof(PackageEntity.Revision)}
            ";
        }
    }
}
