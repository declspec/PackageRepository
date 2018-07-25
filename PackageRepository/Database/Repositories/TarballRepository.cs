using Dapper;
using Fiksu.Database;
using PackageRepository.Models;
using System.Threading.Tasks;

namespace PackageRepository.Database.Repositories {
    /*
    public interface ITarballRepository {
        Task<Tarball> GetByPackageVersionAsync(string package, string version);
        Task CreateAsync(Tarball tarball);
    }

    public class TarballRepository : ITarballRepository {
        private static readonly string SelectByPackageVersionQuery = $"SELECT package, version, data FROM { Tables.PackageTarballs } WHERE package = @Package AND version = @Version";
        private static readonly string CreateQuery = GetCreateQuery();

        private readonly IDbConnectionProvider _connectionProvider;

        public TarballRepository(IDbConnectionProvider connectionProvider) {
            _connectionProvider = connectionProvider;
        }

        public async Task<Tarball> GetByPackageVersionAsync(string package, string version) {
            using(var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false)) {
                return await connection.QuerySingleAsync<Tarball>(SelectByPackageVersionQuery, new { Package = package, Version = version }).ConfigureAwait(false);
            }
        }

        public async Task CreateAsync(Tarball tarball) {
            using(var connection = await _connectionProvider.GetConnectionAsync().ConfigureAwait(false)) {
                await connection.ExecuteAsync(CreateQuery, tarball);
            }
        }

        private static string GetCreateQuery() {
            return $@"INSERT INTO { Tables.PackageTarballs } (package, version, data) VALUES (
                @{nameof(Tarball.Package)},
                @{nameof(Tarball.Version)},
                @{nameof(Tarball.Data)}
            );";
        }
    }*/
}
