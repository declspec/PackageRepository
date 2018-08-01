using System;

namespace PackageRepository.Errors {
    public class PackageException : Exception {
        public int Code { get; }

        public PackageException(int code)
            : this(code, null) { }

        public PackageException(int code, Exception innerException) : base(CodeToString(code), innerException) {
            Code = code;
        }

        private static string CodeToString(int code) {
            return "";
        }
    }
}
