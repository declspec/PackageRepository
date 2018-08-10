using System;

namespace PackageRepository.Exceptions {
    public class PackageNotFoundException : Exception {
        public string Package { get; }

        public PackageNotFoundException(string package)
            : this(package, "package not found", null) { }

        public PackageNotFoundException(string package, string message)
            : this(package, message, null) { }

        public PackageNotFoundException(string package, Exception innerException)
            : this(package, null, innerException) { }

        public PackageNotFoundException(string package, string message, Exception innerException) : base(message, innerException) {
            Package = package;
        }
    }
}
