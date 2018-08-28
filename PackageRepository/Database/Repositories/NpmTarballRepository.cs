using Dapper;
using Fiksu.Database;
using PackageRepository.Database.Entities;
using PackageRepository.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageRepository.Database.Repositories {
    public interface INpmTarballRepository {
        Task SaveAsync(IEnumerable<NpmTarball> tarballs);
        Task DeleteAsync(NpmPackageIdentifier package, IEnumerable<string> versions);
    }

    public class NpmTarballRepository : INpmTarballRepository {
        private static readonly string InsertTarballQuery = GetInsertTarballQuery();
        private static readonly string DeleteTarballQuery = GetDeleteTarballQuery();

        private readonly IDbConnectionProvider _connectionProvider;

        public NpmTarballRepository(IDbConnectionProvider connectionProvider) {
            _connectionProvider = connectionProvider;
        }

        public async Task SaveAsync(IEnumerable<NpmTarball> tarballs) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false))
            using (var transaction = connection.BeginTransaction()) {
                var entities = tarballs.Select(ToEntity).ToList();
                await connection.ExecuteAsync(InsertTarballQuery, entities, transaction).ConfigureAwait(false);
                transaction.Commit();
            }
        }

        public async Task DeleteAsync(NpmPackageIdentifier package, IEnumerable<string> versions) {
            using (var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false))
            using (var transaction = connection.BeginTransaction()) {
                var entities = versions.Select(vers => new NpmTarballEntity() {
                    Organisation = package.Organisation,
                    Package = package.Name,
                    Version = vers
                });

                await connection.ExecuteAsync(DeleteTarballQuery, entities.ToList(), transaction).ConfigureAwait(false);
                transaction.Commit();
            }
        }

        private static NpmTarballEntity ToEntity(NpmTarball model) {
            return model == null ? null : new NpmTarballEntity() {
                Organisation = model.Package.Organisation,
                Package = model.Package.Name,
                Version = model.Version,
                Data = model.Data
            };
        }

        private static string GetInsertTarballQuery() {
            return $@"INSERT INTO { Tables.NpmTarballs } (organisation, package, version, data) VALUES (
                @{nameof(NpmTarballEntity.Organisation)}
                @{nameof(NpmTarballEntity.Package)},
                @{nameof(NpmTarballEntity.Version)},
                @{nameof(NpmTarballEntity.Data)}
            );";
        }

        private static string GetDeleteTarballQuery() {
            return $@"DELETE FROM { Tables.NpmTarballs }
                WHERE organisation = @{nameof(NpmTarballEntity.Organisation)}
                AND package = @{nameof(NpmTarballEntity.Package)}
                AND version = @{nameof(NpmTarballEntity.Version)}";
        }
    }
}
