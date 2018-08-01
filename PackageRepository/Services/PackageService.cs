using PackageRepository.Database.Repositories;
using PackageRepository.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PackageRepository.Services {
    public interface IPackageService {
        Task PublishPackageVersionsAsync(IEnumerable<PublishedPackageVersion> versions);
        Task UnpublishPackageVersionsAsync(IEnumerable<PackageIdentifier> identifiers);
        Task UpdatePackageVersionsAsync(IEnumerable<PackageVersion> versions);
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

        public Task UpdatePackageVersionsAsync(IEnumerable<PackageVersion> versions) {
            return _repository.UpdatePackageVersionsAsync(versions);
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

        private static string ComputeHash(byte[] data) {
            using (var provider = new SHA256CryptoServiceProvider()) {
                return provider.ComputeHash(data)
                    .Aggregate(new StringBuilder(64), (sb, b) => sb.Append(b.ToString("x2")))
                    .ToString();
            }
        }
    }
}
