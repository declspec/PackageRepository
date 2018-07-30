using PackageRepository.Database.Repositories;
using PackageRepository.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PackageRepository.Services {
    public interface IPackageService {
        Task PublishPackageVersionAsync(PublishedPackageVersion package);
        Task UnpublishPackageVersionAsync(PackageIdentifier identifier);
        Task UpdatePackageVersionsAsync(IEnumerable<PackageVersion> versions);
        Task SetDistTagsAsync(string package, IDictionary<string, string> distTags);

        Task<Package> GetPackageAsync(string package);
    }

    public class PackageService : IPackageService {
        private readonly IPackageRepository _repository;

        public PackageService(IPackageRepository repository) {
            _repository = repository;
        }

        public Task PublishPackageVersionAsync(PublishedPackageVersion package) {
            return _repository.CreatePublishedPackageVersionAsync(package);
        }

        public Task UnpublishPackageVersionAsync(PackageIdentifier identifier) {
            throw new System.NotImplementedException();
        }

        public Task UpdatePackageVersionsAsync(IEnumerable<PackageVersion> version) {
            throw new System.NotImplementedException();
        }

        public Task SetDistTagsAsync(string package, IDictionary<string, string> distTags) {
            return _repository.UpdatePackageDistTagsAsync(package, distTags);
        }

        public Task<Package> GetPackageAsync(string package) {
            return _repository.GetPackageAsync(package);
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
