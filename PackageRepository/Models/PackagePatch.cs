using System.Collections.Generic;

namespace PackageRepository.Models
{
    public interface IPackagePatch
    {
        IList<PackageVersion> UpdatedVersions { get; }
        IList<PublishedPackageVersion> PublishedVersions { get; }
        IList<PackageIdentifier> DeletedVersions { get; }
        IDictionary<string, string> UpdatedDistTags { get; }
        IList<string> DeletedDistTags { get; }
    }

    public class PackagePatch : IPackagePatch
    {
        public IList<PackageVersion> UpdatedVersions { get; set; }
        public IList<PublishedPackageVersion> PublishedVersions { get; set; }
        public IList<PackageIdentifier> DeletedVersions { get; set; }
        public IDictionary<string, string> UpdatedDistTags { get; set; }
        public IList<string> DeletedDistTags { get; set; }
    }
}
