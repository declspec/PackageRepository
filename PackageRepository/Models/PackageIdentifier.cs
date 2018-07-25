namespace PackageRepository.Models {
    public class PackageIdentifier {
        public string Name { get; }
        public string Version { get; }

        public PackageIdentifier(string name, string version) {
            Name = name;
            Version = version;
        }
    }
}
