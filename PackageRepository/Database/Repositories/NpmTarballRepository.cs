using Dapper;
using Fiksu.Database;
using PackageRepository.Database.Entities;
using PackageRepository.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageRepository.Database.Repositories {
    public interface INpmTarballRepository {
        Task<NpmTarball> GetAsync(NpmPackageVersionIdentifier identifier);
        Task SaveAsync(IEnumerable<NpmTarball> tarballs);
        Task DeleteAsync(IEnumerable<NpmPackageVersionIdentifier> identifiers);
    }

    public class NpmTarballRepository : INpmTarballRepository {
        private static readonly string SelectTarballQuery = GetSelectTarballQuery();
        private static readonly string InsertTarballQuery = GetInsertTarballQuery();
        private static readonly string DeleteTarballQuery = GetDeleteTarballQuery();

        private readonly IDbConnectionProvider _connectionProvider;

        public NpmTarballRepository(IDbConnectionProvider connectionProvider) {
            _connectionProvider = connectionProvider;
        }

        public async Task<NpmTarball> GetAsync(NpmPackageVersionIdentifier identifier) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false)) {
                var entity = await connection.QuerySingleOrDefaultAsync<NpmTarballEntity>(SelectTarballQuery, identifier).ConfigureAwait(false);
                return ToModel(entity, identifier);
            }
        }

        public async Task SaveAsync(IEnumerable<NpmTarball> tarballs) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false))
            using (var transaction = connection.BeginTransaction()) {
                var entities = tarballs.Select(ToEntity).ToList();
                await connection.ExecuteAsync(InsertTarballQuery, entities, transaction).ConfigureAwait(false);
                transaction.Commit();
            }
        }

        public async Task DeleteAsync(IEnumerable<NpmPackageVersionIdentifier> identifiers) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false))
            using (var transaction = connection.BeginTransaction()) {
                await connection.ExecuteAsync(DeleteTarballQuery, identifiers, transaction).ConfigureAwait(false);
                transaction.Commit();
            }
        }

        private static NpmTarballEntity ToEntity(NpmTarball model) {
            return model == null ? null : new NpmTarballEntity() {
                Organisation = model.Version.Organisation,
                Package = model.Version.Name,
                Version = model.Version.Version,
                Data = model.Data
            };
        }

        private static NpmTarball ToModel(NpmTarballEntity entity, NpmPackageVersionIdentifier identifier = null) {
            return entity == null ? null : new NpmTarball() {
                Data = entity.Data,
                Version = identifier ?? new NpmPackageVersionIdentifier(entity.Organisation, entity.Package, entity.Version),
            };
        }

        private static string GetInsertTarballQuery() {
            return $@"INSERT INTO { Tables.NpmTarballs } (organisation, package, version, data) VALUES (
                @{nameof(NpmTarballEntity.Organisation)},
                @{nameof(NpmTarballEntity.Package)},
                @{nameof(NpmTarballEntity.Version)},
                @{nameof(NpmTarballEntity.Data)}
            );";
        }

        private static string GetDeleteTarballQuery() {
            return $@"DELETE FROM { Tables.NpmTarballs }
                WHERE organisation = @{nameof(NpmPackageVersionIdentifier.Organisation)}
                AND package = @{nameof(NpmPackageVersionIdentifier.Name)}
                AND version = @{nameof(NpmPackageVersionIdentifier.Version)}";
        }

        private static string GetSelectTarballQuery() {
            return $@"SELECT organisation, package, version, data FROM { Tables.NpmTarballs }
                WHERE organisation = @{nameof(NpmPackageVersionIdentifier.Organisation)}
                AND package = @{nameof(NpmPackageVersionIdentifier.Name)}
                AND version = @{nameof(NpmPackageVersionIdentifier.Version)}";
        }
    }
}
