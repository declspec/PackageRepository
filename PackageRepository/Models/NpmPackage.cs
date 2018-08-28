using System.Collections.Generic;

namespace PackageRepository.Models {
    public class NpmPackage {
        public string Name { get; set; }
        public string Revision { get; set; }
        public IDictionary<string, string> DistTags { get; set; }
        public IDictionary<string, Manifest> Versions { get; set; }
    }
}
