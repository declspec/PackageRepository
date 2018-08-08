using PackageRepository.Constants;
using PackageRepository.Database.Repositories;
using PackageRepository.Errors;
using PackageRepository.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageRepository.Services {
    public interface IPackageService {
        Task CommitAsync(string package, IPackageChangeset changeset);
        Task SetDistTagsAsync(string package, IDictionary<string, string> distTags);

        Task<Package> GetPackageAsync(string package);
        Task<Tarball> GetTarballAsync(PackageIdentifier identifier);
    }

    public class PackageService : IPackageService {
        private readonly IPackageRepository _repository;

        public PackageService(IPackageRepository repository) {
            _repository = repository;
        }

        public Task CommitAsync(string package, IPackageChangeset changeset) {
            return changeset.Updated == null || changeset.Updated.Count == 0
                ? _repository.CommitAsync(package, changeset)
                : UpdateAndCommitAsync(package, changeset);
        }

        public Task SetDistTagsAsync(string package, IDictionary<string, string> distTags) {
            return _repository.SetDistTagsAsync(package, distTags);
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
                throw new PackageException(ErrorCodes.PackageNotFound);

            var updatedChangeset = new PackageChangeset() {
                Published = changeset.Published,
                Deleted = changeset.Deleted,
                Updated = changeset.Updated.Select(version => {
                    var matching = overview.Versions.SingleOrDefault(v => v.Id == version.Id)
                        ?? throw new PackageException(ErrorCodes.VersionNotFound);

                    // Only update specific fields
                    matching.Manifest.Deprecated = version.Manifest.Deprecated;

                    return matching;
                }).ToList()
            };

            await _repository.CommitAsync(package, updatedChangeset).ConfigureAwait(false);
        }
    }
}
