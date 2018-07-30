namespace PackageRepository.Models {
    public class PublishedPackageVersion {
        public PackageVersion Version { get; set; }
        public Tarball Tarball { get; set; }
    }
}
