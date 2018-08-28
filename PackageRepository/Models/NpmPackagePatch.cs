using System.Collections.Generic;

namespace PackageRepository.Models {
    public interface INpmPackagePatch {
        NpmPackageIdentifier Identifier { get; }
        IList<string> DeletedVersions { get; }
        IDictionary<string, Manifest> UpdatedVersions { get; }
        IDictionary<string, NpmPublishedPackageVersion> PublishedVersions { get; }
        IDictionary<string, string> UpdatedDistTags { get; }
        IList<string> DeletedDistTags { get; }
    }

    public class NpmPackagePatch : INpmPackagePatch {
        public NpmPackageIdentifier Identifier { get; set; }
        public IList<string> DeletedVersions { get; set; }
        public IDictionary<string, Manifest> UpdatedVersions { get; set; }
        public IDictionary<string, NpmPublishedPackageVersion> PublishedVersions { get; set; }
        public IDictionary<string, string> UpdatedDistTags { get; set; }
        public IList<string> DeletedDistTags { get; set; }
    }
}
