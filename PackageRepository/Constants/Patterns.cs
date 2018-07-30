namespace PackageRepository.Constants {
    public static class Patterns {
        public const string PackageName = @"(?<package>(?:@(?<scope>[a-z0-9-][a-z0-9_.-]*)(?:/|%2[fF]))?(?<name>[a-z0-9-][a-z0-9_.-]*))";
        public const string SemVer = @"(?<version>(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<identifier>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*)?(?:\+[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?)";
    }
}
