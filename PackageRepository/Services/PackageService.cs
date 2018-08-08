using PackageRepository.Database.Repositories;
using PackageRepository.Exceptions;
using PackageRepository.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PackageRepository.Services {
    public interface IPackageService {
        Task CommitAsync(string package, IPackageChangeset changeset);

        Task<Package> GetPackageAsync(string package);
        Task<Tarball> GetTarballAsync(PackageIdentifier identifier);
    }

    public class PackageService : IPackageService {
        private readonly IPackageRepository _repository;

        public PackageService(IPackageRepository repository) {
            _repository = repository;
        }

        public Task CommitAsync(string package, IPackageChangeset changeset) {
            return changeset.UpdatedVersions == null || changeset.UpdatedVersions.Count == 0
                ? _repository.CommitAsync(package, changeset)
                : UpdateAndCommitAsync(package, changeset);
        }

        public Task<Package> GetPackageAsync(string package) {
            return _repository.GetPackageAsync(package);
        }

        public Task<Tarball> GetTarballAsync(PackageIdentifier identifier) {
            return _repository.GetTarballAsync(identifier);
        }

        private async Task UpdateAndCommitAsync(string package, IPackageChangeset changeset) {
            var overview = await _repository.GetPackageAsync(package).ConfigureAwait(false);

            if (overview == null)
                throw new PackageNotFoundException(package);

            var updatedChangeset = new PackageChangeset() {
                PublishedVersions = changeset.PublishedVersions,
                DeletedVersions = changeset.DeletedVersions,
                UpdatedDistTags = changeset.UpdatedDistTags,
                DeletedDistTags = changeset.DeletedDistTags,

                // Ensure that we're only updating allowed fields on the Manifest by cloning the changeset and inverting the updates
                UpdatedVersions = changeset.UpdatedVersions.Select(version => {
                    var matching = overview.Versions.SingleOrDefault(v => v.Id == version.Id)
                        ?? throw new PackageVersionNotFoundException(version.Id);

                    // Treat empty strings as equivalent to null
                    matching.Manifest.Deprecated = string.IsNullOrEmpty(version.Manifest.Deprecated) ? null : version.Manifest.Deprecated;
                    return matching;
                }).ToList()
            };

            await _repository.CommitAsync(package, updatedChangeset).ConfigureAwait(false);
        }
    }
}
