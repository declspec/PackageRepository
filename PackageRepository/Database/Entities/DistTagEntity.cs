namespace PackageRepository.Database.Entities {
    public class DistTagEntity : AuditedEntity {
        public string Package { get; set; }
        public string Tag { get; set; }
        public string Version { get; set; }
    }
}
