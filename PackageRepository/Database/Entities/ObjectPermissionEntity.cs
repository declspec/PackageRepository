using PackageRepository.Enums;

namespace PackageRepository.Database.Entities {
    public class ObjectPermissionEntity {
        public long Id { get; set; }
        public string PermissionType { get; set; }
        public Permissions Permissions { get; set; }
    }
}
