namespace PackageRepository.Models {
    public class PackageIdentifier {
        public string Name { get; }
        public string Version { get; }
        public string TarballName { get => $"{Name}-{Version}.tgz"; }

        public PackageIdentifier(string name, string version) {
            Name = name;
            Version = version;
        }

        public override int GetHashCode() {
            var hash = 17;
            hash = hash * 23 + Name.GetHashCode();
            hash = hash * 23 + Version.GetHashCode();
            return hash;
        }

        public override bool Equals(object obj) {
            return obj is PackageIdentifier id && id.Name == Name && id.Version == Version;
        }

        public static bool operator ==(PackageIdentifier id1, PackageIdentifier id2) {
            return ReferenceEquals(id1, null)
                ? ReferenceEquals(id2, null)
                : id1.Equals(id2);
        }

        public static bool operator !=(PackageIdentifier id1, PackageIdentifier id2) {
            return !(id1 == id2);
        }
    }
}
