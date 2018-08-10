using System;

namespace PackageRepository.Enums {
    [Flags]
    public enum Permission {
        None = 0,
        Create = 0x001,
        Read = 0x0002,
        Update = 0x0004,
        Delete = 0x0008
    }
}
