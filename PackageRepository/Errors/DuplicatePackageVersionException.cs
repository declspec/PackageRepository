using PackageRepository.Models;
using System;

namespace PackageRepository.Errors {
    public class DuplicatePackageVersionException : Exception
    {
        public PackageIdentifier Package { get; }

        public DuplicatePackageVersionException(PackageIdentifier identifier, string message)
            : this(identifier, message, null) { }

        public DuplicatePackageVersionException(PackageIdentifier identifier, Exception innerException)
            : this(identifier, null, innerException) { }

        public DuplicatePackageVersionException(PackageIdentifier identifier, string message, Exception innerException) : base(message, innerException) {
            Package = identifier;
        }
    }
}
