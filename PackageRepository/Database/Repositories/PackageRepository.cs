using Dapper;
using Fiksu.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PackageRepository.Database.Entities;
using PackageRepository.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PackageRepository.Database.Repositories {
    public interface IPackageRepository {
        Task CreateVersionAsync(CreatePackageVersion model);
    }

    public class PackageRepository : IPackageRepository {
        private readonly JsonSerializerSettings DefaultSerializationSettings = new JsonSerializerSettings() {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver() {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };

        private static readonly string CreateDistTagQuery = GetCreateDistTagQuery();
        private static readonly string CreatePackageVersionQuery = GetCreatePackageVersionQuery();

        private readonly IDbConnectionProvider _connectionProvider;

        public PackageRepository(IDbConnectionProvider connectionProvider) {
            _connectionProvider = connectionProvider;
        }

        public async Task CreateVersionAsync(CreatePackageVersion model) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false))
            using (var transaction = connection.BeginTransaction()) {
                var version = new PackageVersionEntity() {
                    Name = model.Name,
                    Version = model.Version,
                    Manifest = JsonConvert.SerializeObject(model.Manifest, DefaultSerializationSettings)
                };

                var tasks = model.DistTags.Select(kvp => {
                    var tag = new DistTagEntity() { Package = model.Name, Tag = kvp.Key, Version = kvp.Value };
                    return connection.ExecuteAsync(CreateDistTagQuery, tag, transaction);
                }).Concat(new[] { connection.ExecuteAsync(CreatePackageVersionQuery, version, transaction) });

                
                await Task.WhenAll(tasks).ConfigureAwait(false);
                transaction.Commit();
            }
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
            return $@"INSERT INTO { Tables.PackageVersions } (name, version, manifest) VALUES (
                @{nameof(PackageVersionEntity.Name)},
                @{nameof(PackageVersionEntity.Version)},
                @{nameof(PackageVersionEntity.Manifest)}
            );";
        }
    }
}
