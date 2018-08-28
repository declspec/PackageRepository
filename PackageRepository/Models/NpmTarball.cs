namespace PackageRepository.Models {
    public class NpmTarball {
        public NpmPackageIdentifier Package { get; set; }
        public string Version { get; set; }
        public byte[] Data { get; set; }
    }
}
