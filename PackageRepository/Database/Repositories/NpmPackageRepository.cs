using Dapper;
using Fiksu.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using PackageRepository.Database.Entities;
using PackageRepository.Models;
using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace PackageRepository.Database.Repositories {
    public interface INpmPackageRepository {
        Task<NpmPackage> GetPackageAsync(string organisation, string packageName);
        Task<string> SaveAsync(string organisation, NpmPackage package);
    }

    public class NpmPackageRepository : INpmPackageRepository {
        private const string SelectPackageQuery = "SELECT organisation, name, revision, package FROM " + Tables.NpmPackages + " WHERE organisation = @Organisation AND name = @Name";
        private static readonly string UpdatePackageQuery = GetUpdatePackageQuery();
        private static readonly JsonSerializer Serializer = new JsonSerializer();

        private readonly IDbConnectionProvider _connectionProvider;

        public NpmPackageRepository(IDbConnectionProvider connectionProvider) {
            _connectionProvider = connectionProvider;
        }

        public async Task<NpmPackage> GetPackageAsync(string organisation, string packageName) {
            using(var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false)) {
                var param = new { Organisation = organisation, Name = packageName };
                var entity = await connection.QuerySingleOrDefaultAsync<NpmPackageEntity>(SelectPackageQuery, param).ConfigureAwait(false);
                return ToModel(entity);
            }
        }

        public async Task<string> SaveAsync(string organisation, NpmPackage package) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false)) {
                var param = new {
                    Organisation = organisation,
                    Name = package.Name,
                    Package = SerializePackage(package),
                    Revision = package.Revision,
                    NextRevision = Guid.NewGuid().ToString()
                };

                var updated = await connection.ExecuteAsync(UpdatePackageQuery, param).ConfigureAwait(false);

                if (updated != 1)
                    throw new Exception("Conflict encountered");

                return param.NextRevision;
            }
        }

        private Task<NpmPackage> GetPackageAsync(IDbConnection connection, string organisation, string packageName) {
            return connection.QuerySingleOrDefaultAsync<NpmPackageEntity>(SelectPackageQuery, new { Organisation = organisation, Name = packageName })
                .ContinueWith(t => ToModel(t.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
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

        private static byte[] SerializePackage(NpmPackage package) {
            using (var ms = new MemoryStream())
            using(var writer = new BsonDataWriter(ms)) {
                Serializer.Serialize(writer, package);
                return ms.ToArray();
            }
        }

        private static string GetUpdatePackageQuery() {
            return $@"UPDATE {Tables.NpmPackages} SET
                revision = @NextRevision,
                package = @Package,
                date_modified = CURRENT_TIMESTAMP
                WHERE organisation = @Organisation
                AND name = @Name
                AND revision = @Revision
            ";
        }
    }
}
