using Dapper;
using Fiksu.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using PackageRepository.Database.Entities;
using PackageRepository.Exceptions;
using PackageRepository.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PackageRepository.Database.Repositories {
    public interface INpmPackageRepository {
        Task<NpmPackage> GetAsync(NpmPackageIdentifier identifier);
        Task<string> CreateAsync(NpmPackage package);
        Task<string> UpdateAsync(NpmPackage package);
    }

    public class NpmPackageRepository : INpmPackageRepository {
        private const string SelectPackageQuery = "SELECT organisation, name, revision, package FROM " + Tables.NpmPackages + " WHERE organisation = @Organisation AND name = @Name";
        private static readonly string UpdatePackageQuery = GetUpdatePackageQuery();
        private static readonly string InsertPackageQuery = GetInsertPackageQuery();

        private static readonly JsonSerializer Serializer = new JsonSerializer();

        private readonly IDbConnectionProvider _connectionProvider;

        public NpmPackageRepository(IDbConnectionProvider connectionProvider) {
            _connectionProvider = connectionProvider;
        }

        public async Task<NpmPackage> GetAsync(NpmPackageIdentifier identifier) {
            using(var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false)) {
                var entity = await connection.QuerySingleOrDefaultAsync<NpmPackageEntity>(SelectPackageQuery, identifier).ConfigureAwait(false);
                return ToModel(entity);
            }
        }

        public async Task<string> CreateAsync(NpmPackage package) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false)) {
                var entity = ToEntity(package);
                await connection.ExecuteAsync(InsertPackageQuery, entity).ConfigureAwait(false);
                return entity.NextRevision;
            }
        }

        public async Task<string> UpdateAsync(NpmPackage package) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false)) {
                var entity = ToEntity(package);
                var updated = await connection.ExecuteAsync(UpdatePackageQuery, entity).ConfigureAwait(false);

                if (updated != 1)
                    throw new PackageConflictException(new NpmPackageIdentifier(package.Organisation, package.Name));

                return entity.NextRevision;
            }
        }

        private static NpmPackage ToModel(NpmPackageEntity entity) {
            if (entity == null)
                return null;

            using (var ms = new MemoryStream(entity.Package))
            using (var reader = new BsonDataReader(ms)) {
                var model = Serializer.Deserialize<NpmPackage>(reader);
                model.Revision = entity.Revision;

                return model;
            }
        }

        private static NpmPackageEntity ToEntity(NpmPackage model) {
            return model == null ? null : new NpmPackageEntity() {
                Name = model.Name,
                Organisation = model.Organisation,
                Revision = model.Revision,
                Package = SerializePackage(model),
                NextRevision = Guid.NewGuid().ToString()
            };
        }

        private static byte[] SerializePackage(NpmPackage package) {
            using (var ms = new MemoryStream())
            using(var writer = new BsonDataWriter(ms)) {
                Serializer.Serialize(writer, package);
                return ms.ToArray();
            }
        }

        private static string GetInsertPackageQuery() {
            return $@"INSERT INTO { Tables.NpmPackages } (organisation, name, package, revision) VALUES (
                @{nameof(NpmPackageEntity.Organisation)},
                @{nameof(NpmPackageEntity.Name)},
                @{nameof(NpmPackageEntity.Package)},
                @{nameof(NpmPackageEntity.NextRevision)}
            );";
        }

        private static string GetUpdatePackageQuery() {
            return $@"UPDATE {Tables.NpmPackages} SET
                revision = @{nameof(NpmPackageEntity.NextRevision)},
                package = @{nameof(NpmPackageEntity.Package)},
                date_modified = CURRENT_TIMESTAMP
                WHERE organisation = @{nameof(NpmPackageEntity.Organisation)}
                AND name = @{nameof(NpmPackageEntity.Name)}
                AND revision = @{nameof(NpmPackageEntity.Revision)}
            ";
        }
    }
}
