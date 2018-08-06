using System;
using PackageRepository.Models;

namespace PackageRepository.Utils {
    public static class PackageUtils {
        public static string GetTarballName(string package, string version) {
            return $"{package}-{version}.tgz";
        }

        public static string GetTarballName(PackageIdentifier identifier) {
            return identifier != null 
                ? GetTarballName(identifier.Name, identifier.Version) 
                : throw new ArgumentNullException(nameof(identifier));
        }

        public static string UnescapeName(string packageName) {
            return packageName.Replace("%2f", "/", StringComparison.OrdinalIgnoreCase)
                .Replace("%40", "@", StringComparison.Ordinal);
        }
    }
}
