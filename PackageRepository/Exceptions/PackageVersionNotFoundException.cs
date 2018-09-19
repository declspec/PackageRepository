using System;
using PackageRepository.Models;

namespace PackageRepository.Exceptions {
    public class PackageVersionNotFoundException : Exception {
        public NpmPackageVersionIdentifier Identifier { get; }

        public PackageVersionNotFoundException(NpmPackageVersionIdentifier identifier)
            : this(identifier, "Package version not found", null) { }

        public PackageVersionNotFoundException(NpmPackageVersionIdentifier identifier, string message)
            : this(identifier, message, null) { }

        public PackageVersionNotFoundException(NpmPackageVersionIdentifier identifier, Exception innerException)
            : this(identifier, null, innerException) { }

        public PackageVersionNotFoundException(NpmPackageVersionIdentifier identifier, string message, Exception innerException) : base(message, innerException) {
            Identifier = identifier;
        }
    }
}
