using System;

namespace GetPkg.Npm.Exceptions {
    public class DuplicatePackageVersionException : Exception {
        public PackageVersionIdentifier Identifier { get; }

        public DuplicatePackageVersionException(PackageVersionIdentifier identifier)
            : this(identifier, "Duplicate package version found") { }

        public DuplicatePackageVersionException(PackageVersionIdentifier identifier, string message)
            : this(identifier, message, null) { }

        public DuplicatePackageVersionException(PackageVersionIdentifier identifier, Exception innerException)
            : this(identifier, null, innerException) { }

        public DuplicatePackageVersionException(PackageVersionIdentifier identifier, string message, Exception innerException) : base(message, innerException) {
            Identifier = identifier;
        }
    }
}
