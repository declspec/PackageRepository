namespace PackageRepository.Database.Entities {
    public class PackageVersionEntity : AuditedEntity {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Manifest { get; set; }
        public bool Published { get; set; }
    }
}
