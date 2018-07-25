namespace PackageRepository.Models {
    public class Tarball {
        public PackageIdentifier Package { get; set; }
        public byte[] Data { get; set; }
    }
}
