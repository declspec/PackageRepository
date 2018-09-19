namespace GetPkg.Npm {
    public class PackageIdentifier {
        public string Organisation { get; }
        public string Name { get; }

        public PackageIdentifier(string organisation, string name) {
            Organisation = organisation;
            Name = name;
        }

        public override int GetHashCode() {
            return (Organisation, Name).GetHashCode();
        }

        public override bool Equals(object obj) {
            return obj is PackageIdentifier id
                && id.Name == Name
                && id.Organisation == Organisation;
        }

        public static bool operator ==(PackageIdentifier id1, PackageIdentifier id2) {
            return id1 is null ? id2 is null : id1.Equals(id2);
        }

        public static bool operator !=(PackageIdentifier id1, PackageIdentifier id2) {
            return !(id1 == id2);
        }
    }
}
