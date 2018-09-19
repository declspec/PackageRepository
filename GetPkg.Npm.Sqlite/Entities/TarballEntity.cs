using System;

namespace GetPkg.Npm.Sqlite.Entities {
    public class TarballEntity {
        public string Organisation { get; set; }
        public string Package { get; set; }
        public string Version { get; set; }
        public byte[] Data { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateModified { get; set; }
    }
}
