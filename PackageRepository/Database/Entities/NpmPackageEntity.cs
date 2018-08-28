namespace PackageRepository.Database.Entities {
    public class NpmPackageEntity {
        public string Organisation { get; set; }
        public string Name { get; set; }
        public string Revision { get; set; }
        public byte[] Package { get; set; }
    }
}
