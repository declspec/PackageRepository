using PackageRepository.Database.Repositories;
using PackageRepository.Exceptions;
using PackageRepository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageRepository.Services {
    public interface INpmPackageService {
        Task<string> CommitAsync(INpmPackagePatch patch);
        Task<NpmPackage> GetPackageAsync(NpmPackageIdentifier identifier);
    }

    public class NpmPackageService : INpmPackageService {
        private readonly INpmPackageRepository _packageRepository;
        private readonly INpmTarballRepository _tarballRepository;

        public NpmPackageService(INpmPackageRepository packageRepository, INpmTarballRepository tarballRepository) {
            _packageRepository = packageRepository;
            _tarballRepository = tarballRepository;
        }

        public Task<NpmPackage> GetPackageAsync(NpmPackageIdentifier identifier) {
            return _packageRepository.GetPackageAsync(identifier.Organisation, identifier.Name);
        }

        public async Task<string> CommitAsync(INpmPackagePatch patch) {
            var package = await _packageRepository.GetPackageAsync(patch.Identifier.Organisation, patch.Identifier.Name).ConfigureAwait(false);
            var tarballs = new List<NpmTarball>();

            if (package == null)
                throw new PackageNotFoundException(patch.Identifier.Name);

            // Deletes
            if (patch.DeletedVersions?.Count > 0) {
                foreach(var version in patch.DeletedVersions) {
                    if (!package.Versions.TryGetValue(version, out var manifest))
                        throw new PackageVersionNotFoundException(null, "not found"); // TODO: fix this exception
                    package.Versions[version] = null; // do NOT actually remove it; this prevents duplicate publishing of the same version.
                }
            }

            // Updates
            if (patch.UpdatedVersions?.Count > 0) {
                foreach (var kvp in patch.UpdatedVersions) {
                    if (!package.Versions.TryGetValue(kvp.Key, out var manifest) || manifest == null)
                        throw new PackageVersionNotFoundException(null, "not found"); // TODO: fix this exception

                    // Update the allowed values
                    manifest.Deprecated = kvp.Value.Deprecated;
                }
            }

            // Publishes
            if (patch.PublishedVersions?.Count > 0) {
                foreach (var kvp in patch.PublishedVersions) {
                    if (!package.Versions.TryAdd(kvp.Key, kvp.Value.Manifest))
                        throw new DuplicatePackageVersionException(null, "duplicate"); // TODO: fix this exception

                    tarballs.Add(new NpmTarball() {
                        Package = patch.Identifier,
                        Version = kvp.Key,
                        Data = kvp.Value.Data
                    });
                }
            }

            // New/updated dist tags
            if (patch.UpdatedDistTags?.Count > 0) {
                foreach(var kvp in patch.UpdatedDistTags) 
                    package.DistTags[kvp.Key] = kvp.Value;
            }

            // Removed dist tags
            if (patch.DeletedDistTags?.Count > 0) {
                foreach (var tag in patch.DeletedDistTags)
                    package.DistTags.Remove(tag);
            }

            // Attempt to save the tarballs first as they're easier to rollback
            if (tarballs.Count > 0)
                await _tarballRepository.SaveAsync(tarballs).ConfigureAwait(false);

            try {
                return await _packageRepository.SaveAsync(patch.Identifier.Organisation, package).ConfigureAwait(false);
            }
            catch(Exception) when (tarballs.Count > 0) {
                await _tarballRepository.DeleteAsync(patch.Identifier, tarballs.Select(t => t.Version)).ConfigureAwait(false);
                throw;
            }
        }
    }
}
