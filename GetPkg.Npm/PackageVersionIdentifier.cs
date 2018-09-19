namespace GetPkg.Npm {
    public class PackageVersionIdentifier : PackageIdentifier {
        public string Version { get; }

        public PackageVersionIdentifier(string organisation, string name, string version) : base(organisation, name) {
            Version = version;
        }

        public override int GetHashCode() {
            return (Organisation, Name, Version).GetHashCode();
        }

        public override bool Equals(object obj) {
            return obj is PackageVersionIdentifier id
                && id.Name == Name
                && id.Organisation == Organisation
                && id.Version == Version;
        }

        public static bool operator ==(PackageVersionIdentifier id1, PackageVersionIdentifier id2) {
            return id1 is null ? id2 is null : id1.Equals(id2);
        }

        public static bool operator !=(PackageVersionIdentifier id1, PackageVersionIdentifier id2) {
            return !(id1 == id2);
        }
    }
}
