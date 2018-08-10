using System.Collections.Generic;

namespace PackageRepository.Models {
    public class CreatePackageVersion {
        public string Name { get; set; }
        public string Version { get; set; }
        public Manifest Manifest { get; set; }
        public IDictionary<string, string> DistTags { get; set; }
    }
}
