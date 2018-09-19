using System.Threading.Tasks;

namespace GetPkg.Npm {
    public interface INpmPackageRepository {
        Task<Package> GetAsync(PackageIdentifier identifier);
        Task<string> CreateAsync(Package package);
        Task<string> UpdateAsync(Package package);
    }
}
