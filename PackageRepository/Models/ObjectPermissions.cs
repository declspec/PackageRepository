﻿using PackageRepository.Enums;
using System.Collections.Generic;

namespace PackageRepository.Models {
    public class ObjectPermissions {
        public IDictionary<long, Permissions> Organisations { get; set; }
        public IDictionary<long, Permissions> Teams { get; set; }
        public IDictionary<long, Permissions> Users { get; set; }
    }
}
