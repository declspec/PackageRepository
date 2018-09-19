using System.Collections.Generic;

namespace GetPkg.Npm {
    public class Package {
        public string Organisation { get; set; }
        public string Name { get; set; }
        public string Revision { get; set; }
        public IDictionary<string, string> DistTags { get; set; }
        public IDictionary<string, Manifest> Versions { get; set; }
    }
}
