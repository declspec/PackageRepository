using System.Collections.Generic;

namespace PackageRepository.Models {
    public class PackageOverview
    {
        public string Name { get; set; }
        public IDictionary<string, string> DistTags { get; set; }
        public IList<PackageVersion> Versions { get; set; }
    }
}
