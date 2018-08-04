using PackageRepository.Constants;
using PackageRepository.Database.Repositories;
using PackageRepository.Errors;
using PackageRepository.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageRepository.Services {
    public interface IPackageService {
        Task PublishPackageVersionsAsync(IEnumerable<PublishedPackageVersion> versions);
        Task UnpublishPackageVersionsAsync(IEnumerable<PackageIdentifier> identifiers);
        Task UpdatePackageVersionsAsync(string package, IEnumerable<PackageVersion> versions);
        Task SetDistTagsAsync(string package, IDictionary<string, string> distTags);

        Task<Package> GetPackageAsync(string package);
        Task<Tarball> GetTarballAsync(PackageIdentifier identifier);
    }

    public class PackageService : IPackageService {
        private readonly IPackageRepository _repository;

        public PackageService(IPackageRepository repository) {
            _repository = repository;
        }

        public Task PublishPackageVersionsAsync(IEnumerable<PublishedPackageVersion> versions) {
            return _repository.PublishPackageVersionsAsync(versions);
        }

        public Task UnpublishPackageVersionsAsync(IEnumerable<PackageIdentifier> identifiers) {
            return _repository.UnpublishPackageVersionsAsync(identifiers);
        }

        public async Task UpdatePackageVersionsAsync(string package, IEnumerable<PackageVersion> versions) {
            var overview = await _repository.GetPackageAsync(package).ConfigureAwait(false);

            if (overview == null)
                throw new PackageException(ErrorCodes.PackageNotFound);

            await _repository.UpdatePackageVersionsAsync(versions.Select(version => {
                var matching = overview.Versions.SingleOrDefault(v => v.Id == version.Id)
                    ?? throw new PackageException(ErrorCodes.VersionNotFound);

                // Only update specific fields
                matching.Manifest.Deprecated = version.Manifest.Deprecated;

                return matching;
            }));
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
    }
}
