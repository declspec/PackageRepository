using System.Collections.Generic;
using System.Threading.Tasks;

namespace GetPkg.Npm {
    public interface INpmTarballRepository {
        Task<Tarball> GetAsync(PackageVersionIdentifier identifier);
        Task SaveAsync(IEnumerable<Tarball> tarballs);
        Task DeleteAsync(IEnumerable<PackageVersionIdentifier> identifiers);
    }
}
