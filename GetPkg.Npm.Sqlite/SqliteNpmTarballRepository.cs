using Dapper;
using Fiksu.Database;
using GetPkg.Npm.Sqlite.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetPkg.Npm.Sqlite {
    public class SqliteTarballRepository : INpmTarballRepository {
        private static readonly string SelectTarballQuery = GetSelectTarballQuery();
        private static readonly string InsertTarballQuery = GetInsertTarballQuery();
        private static readonly string DeleteTarballQuery = GetDeleteTarballQuery();

        private readonly IDbConnectionProvider _connectionProvider;

        public SqliteTarballRepository(IDbConnectionProvider connectionProvider) {
            _connectionProvider = connectionProvider;
        }

        public async Task<Tarball> GetAsync(PackageVersionIdentifier identifier) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false)) {
                var entity = await connection.QuerySingleOrDefaultAsync<TarballEntity>(SelectTarballQuery, identifier).ConfigureAwait(false);
                return ToModel(entity, identifier);
            }
        }

        public async Task SaveAsync(IEnumerable<Tarball> tarballs) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false))
            using (var transaction = connection.BeginTransaction()) {
                var entities = tarballs.Select(ToEntity).ToList();
                await connection.ExecuteAsync(InsertTarballQuery, entities, transaction).ConfigureAwait(false);
                transaction.Commit();
            }
        }

        public async Task DeleteAsync(IEnumerable<PackageVersionIdentifier> identifiers) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false))
            using (var transaction = connection.BeginTransaction()) {
                await connection.ExecuteAsync(DeleteTarballQuery, identifiers, transaction).ConfigureAwait(false);
                transaction.Commit();
            }
        }

        private static TarballEntity ToEntity(Tarball model) {
            return model == null ? null : new TarballEntity() {
                Organisation = model.Version.Organisation,
                Package = model.Version.Name,
                Version = model.Version.Version,
                Data = model.Data
            };
        }

        private static Tarball ToModel(TarballEntity entity, PackageVersionIdentifier identifier = null) {
            return entity == null ? null : new Tarball() {
                Data = entity.Data,
                Version = identifier ?? new PackageVersionIdentifier(entity.Organisation, entity.Package, entity.Version),
            };
        }

        private static string GetInsertTarballQuery() {
            return $@"INSERT INTO { Tables.Tarballs } (organisation, package, version, data) VALUES (
                @{nameof(TarballEntity.Organisation)},
                @{nameof(TarballEntity.Package)},
                @{nameof(TarballEntity.Version)},
                @{nameof(TarballEntity.Data)}
            );";
        }

        private static string GetDeleteTarballQuery() {
            return $@"DELETE FROM { Tables.Tarballs }
                WHERE organisation = @{nameof(PackageVersionIdentifier.Organisation)}
                AND package = @{nameof(PackageVersionIdentifier.Name)}
                AND version = @{nameof(PackageVersionIdentifier.Version)}";
        }

        private static string GetSelectTarballQuery() {
            return $@"SELECT organisation, package, version, data FROM { Tables.Tarballs }
                WHERE organisation = @{nameof(PackageVersionIdentifier.Organisation)}
                AND package = @{nameof(PackageVersionIdentifier.Name)}
                AND version = @{nameof(PackageVersionIdentifier.Version)}";
        }
    }
}
