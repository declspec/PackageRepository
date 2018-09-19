namespace PackageRepository.Models {
    public class NpmTarball {
        public NpmPackageVersionIdentifier Version { get; set; }
        public byte[] Data { get; set; }
    }
}
