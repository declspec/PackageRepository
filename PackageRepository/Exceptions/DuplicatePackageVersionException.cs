using PackageRepository.Models;
using System;

namespace PackageRepository.Exceptions {
    public class DuplicatePackageVersionException : Exception {
        public NpmPackageVersionIdentifier Identifier { get; }

        public DuplicatePackageVersionException(NpmPackageVersionIdentifier identifier)
            : this(identifier, "Duplicate package version found") { }

        public DuplicatePackageVersionException(NpmPackageVersionIdentifier identifier, string message)
            : this(identifier, message, null) { }

        public DuplicatePackageVersionException(NpmPackageVersionIdentifier identifier, Exception innerException)
            : this(identifier, null, innerException) { }

        public DuplicatePackageVersionException(NpmPackageVersionIdentifier identifier, string message, Exception innerException) : base(message, innerException) {
            Identifier = identifier;
        }
    }
}
