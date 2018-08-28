namespace PackageRepository.Database.Entities {
    public class NpmTarballEntity : AuditedEntity {
        public string Organisation { get; set; }
        public string Package { get; set; }
        public string Version { get; set; }
        public byte[] Data { get; set; }
    }
}
