using System;
using PackageRepository.Models;

namespace PackageRepository.Exceptions {
    public class PackageVersionNotFoundException : Exception {
        public PackageIdentifier Identifier { get; }

        public PackageVersionNotFoundException(PackageIdentifier identifier)
            : this(identifier, "package version not found", null) { }

        public PackageVersionNotFoundException(PackageIdentifier identifier, string message)
            : this(identifier, message, null) { }

        public PackageVersionNotFoundException(PackageIdentifier identifier, Exception innerException)
            : this(identifier, null, innerException) { }

        public PackageVersionNotFoundException(PackageIdentifier identifier, string message, Exception innerException) : base(message, innerException) {
            Identifier = identifier;
        }
    }
}
