using System;

namespace GetPkg.Npm.Exceptions {
    public class PackageConflictException : Exception {
        public PackageIdentifier Identifier { get; }

        public PackageConflictException(PackageIdentifier identifier)
            : this(identifier, "An unresolvable conflict occurred during package update", null) { }

        public PackageConflictException(PackageIdentifier identifier, string message)
            : this(identifier, message, null) { }

        public PackageConflictException(PackageIdentifier identifier, Exception innerException)
            : this(identifier, null, innerException) { }

        public PackageConflictException(PackageIdentifier identifier, string message, Exception innerException) : base(message, innerException) {
            Identifier = identifier;
        }
    }
}
