using PackageRepository.Enums;

namespace PackageRepository.Database.Entities {
    public class ObjectPermissionEntity {
        public int Id { get; set; }
        public string PermissionType { get; set; }
        public Permission Permissions { get; set; }
    }
}
