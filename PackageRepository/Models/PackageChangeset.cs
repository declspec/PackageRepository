using System;
using System.Collections.Generic;
using System.Text;

namespace PackageRepository.Models
{
    public interface IPackageChangeset
    {
        IList<PackageVersion> Updated { get; }
        IList<PublishedPackageVersion> Published { get; }
        IList<PackageIdentifier> Deleted { get; }
    }

    public class PackageChangeset : IPackageChangeset
    {
        public IList<PackageVersion> Updated { get; set; }
        public IList<PublishedPackageVersion> Published { get; set; }
        public IList<PackageIdentifier> Deleted { get; set; }
    }
}
