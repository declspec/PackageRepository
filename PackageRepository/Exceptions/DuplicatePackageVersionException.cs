using PackageRepository.Models;
using System;

namespace PackageRepository.Exceptions {
    public class DuplicatePackageVersionException : Exception {
        public PackageIdentifier Identifier { get; }

        public DuplicatePackageVersionException(PackageIdentifier identifier, string message)
            : this(identifier, message, null) { }

        public DuplicatePackageVersionException(PackageIdentifier identifier, Exception innerException)
            : this(identifier, null, innerException) { }

        public DuplicatePackageVersionException(PackageIdentifier identifier, string message, Exception innerException) : base(message, innerException) {
            Identifier = identifier;
        }
    }
}
