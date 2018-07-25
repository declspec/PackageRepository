using PackageRepository.Database.Repositories;
using PackageRepository.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PackageRepository.Services {
    public interface IPackageService {
        Task PublishPackageAsync(PublishedPackage package);
        Task UnpublishPackageAsync(PackageIdentifier identifier);
        Task DeprecatePackageAsync(PackageIdentifier identifier, string message);

        Task<PackageOverview> GetPackageOverviewAsync(string package);
    }

    public class PackageService : IPackageService {
        private readonly IPackageRepository _repository;

        public PackageService(IPackageRepository repository) {
            _repository = repository;
        }

        public Task PublishPackageAsync(PublishedPackage package) {
            return _repository.CreatePublishedPackageAsync(package);
        }

        public Task UnpublishPackageAsync(PackageIdentifier identifier) {
            throw new System.NotImplementedException();
        }

        public Task DeprecatePackageAsync(PackageIdentifier identifier, string message) {
            throw new System.NotImplementedException();
        }

        public Task<PackageOverview> GetPackageOverviewAsync(string package) {
            return _repository.GetPackageOverviewAsync(package);
        }

        private static string ComputeHash(byte[] data) {
            using(var provider = new SHA256CryptoServiceProvider()) {
                return provider.ComputeHash(data)
                    .Aggregate(new StringBuilder(64), (sb, b) => sb.Append(b.ToString("x2")))
                    .ToString();
            }
        }
    }
}
