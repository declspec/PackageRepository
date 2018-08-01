namespace PackageRepository.Database.Entities {
    public class TarballEntity : AuditedEntity {
        public string Package { get; set; }
        public string Version { get; set; }
        public byte[] Data { get; set; }
    }
}
