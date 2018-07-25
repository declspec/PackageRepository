using System.Collections.Generic;

namespace PackageRepository.Models {
    public class PublishedPackage {
        public PackageVersion Version { get; set; }
        public IDictionary<string, string> DistTags { get; set; }
        public Tarball Tarball { get; set; }
    }
}
