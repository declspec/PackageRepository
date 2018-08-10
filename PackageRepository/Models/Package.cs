using System.Collections.Generic;

namespace PackageRepository.Models {
    public class Package {
        public string Name { get; set; }
        public IDictionary<string, string> DistTags { get; set; }
        public IList<PackageVersion> Versions { get; set; }
    }
}
