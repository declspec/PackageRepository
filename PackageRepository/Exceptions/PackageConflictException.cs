using PackageRepository.Models;
using System;

namespace PackageRepository.Exceptions {
    public class PackageConflictException : Exception {
        public NpmPackageIdentifier Identifier { get; }

        public PackageConflictException(NpmPackageIdentifier identifier)
            : this(identifier, "An unresolvable conflict occurred during package update", null) { }

        public PackageConflictException(NpmPackageIdentifier identifier, string message)
            : this(identifier, message, null) { }

        public PackageConflictException(NpmPackageIdentifier identifier, Exception innerException)
            : this(identifier, null, innerException) { }

        public PackageConflictException(NpmPackageIdentifier identifier, string message, Exception innerException) : base(message, innerException) {
            Identifier = identifier;
        }
    }
}
