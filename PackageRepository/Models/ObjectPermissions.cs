using PackageRepository.Enums;
using System.Collections.Generic;

namespace PackageRepository.Models {
    public class ObjectPermissions {
        public IDictionary<int, Permission> Organisations { get; set; }
        public IDictionary<int, Permission> Teams { get; set; }
        public IDictionary<int, Permission> Users { get; set; }
    }
}
