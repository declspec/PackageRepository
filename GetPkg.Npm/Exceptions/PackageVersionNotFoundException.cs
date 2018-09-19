using System;

namespace GetPkg.Npm.Exceptions {
    public class PackageVersionNotFoundException : Exception {
        public PackageVersionIdentifier Identifier { get; }

        public PackageVersionNotFoundException(PackageVersionIdentifier identifier)
            : this(identifier, "Package version not found", null) { }

        public PackageVersionNotFoundException(PackageVersionIdentifier identifier, string message)
            : this(identifier, message, null) { }

        public PackageVersionNotFoundException(PackageVersionIdentifier identifier, Exception innerException)
            : this(identifier, null, innerException) { }

        public PackageVersionNotFoundException(PackageVersionIdentifier identifier, string message, Exception innerException) : base(message, innerException) {
            Identifier = identifier;
        }
    }
}
