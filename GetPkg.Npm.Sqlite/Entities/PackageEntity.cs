using System;

namespace GetPkg.Npm.Sqlite.Entities {
    public class PackageEntity {
        public string Organisation { get; set; }
        public string Name { get; set; }
        public string Revision { get; set; }
        public byte[] Package { get; set; }
        public string NextRevision { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateModified { get; set; }
    }
}
