using System.Collections.Generic;

namespace GetPkg.Npm {
    public class Manifest {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string Main { get; set; }
        public bool Private { get; set; }
        public string Homepage { get; set; }
        public bool Flat { get; set; }
        public string License { get; set; }
        public string LicenseText { get; set; }
        public string NoticeText { get; set; }
        public string Deprecated { get; set; }
        public string Readme { get; set; }
        public string ReadmeFilename { get; set; }

        public Person Author { get; set; }
        public IList<Person> Maintainers { get; set; }

        public IList<string> Man { get; set; }
        public IList<string> Os { get; set; }
        public IList<string> Cpu { get; set; }

        public IDictionary<string, string> Bin { get; set; }
        public IDictionary<string, string> Engines { get; set; }
        public IDictionary<string, string> Scripts { get; set; }

        public IList<string> BundledDependencies { get; set; }
        public IDictionary<string, string> Dependencies { get; set; }
        public IDictionary<string, string> DevDependencies { get; set; }
        public IDictionary<string, string> OptionalDependencies { get; set; }
        public IDictionary<string, string> PeerDependencies { get; set; }
        public IDictionary<string, string> PrebuiltVariants { get; set; }

        public RepositoryMetadata Repository { get; set; }
        public BugMetadata Bugs { get; set; }
        public DistributionMetadata Dist { get; set; }
    }

    public class DistributionMetadata {
        public string Tarball { get; set; }
        public string Shasum { get; set; }
        public string Integrity { get; set; }
    }

    public class RepositoryMetadata {
        public string Type { get; set; }
        public string Url { get; set; }
    }

    public class BugMetadata {
        public string Url { get; set; }
        public string Email { get; set; }
    }

    public class DirectoriesMetadata {
        public string Man { get; set; }
        public string Bin { get; set; }
    }

    public class Person {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }
}
