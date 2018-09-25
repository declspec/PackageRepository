using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FiksuCore.Web.Routing;
using GetPkg.Npm;
using Microsoft.AspNetCore.Mvc;
using PackageRepository.Constants;
using PackageRepository.ViewModels.Npm;

namespace PackageRepository.Controllers.Npm {
    [RegexRoute(Patterns.OrganisationName + "/npm/" + Patterns.PackageName)]
    public class PackageController : NpmControllerBase {
        private const string TarballRoute = @"-/\k<package>-" + Patterns.SemVer + @"\.tgz";
        private const string RevisionRoute = "-rev/(?<revision>.+)";

        private static readonly IActionResult PackageNotFoundResponse = Error("package not found", HttpStatusCode.NotFound);
        private static readonly IActionResult DuplicateVersionResponse = Error("cannot overwrite existing package version", HttpStatusCode.NotAcceptable);

        private readonly INpmPackageRepository _packageRepository;
        private readonly INpmTarballRepository _tarballRepository;

        public PackageController(INpmPackageRepository packageRepository, INpmTarballRepository tarballRepository) {
            _packageRepository = packageRepository;
            _tarballRepository = tarballRepository;
        }

        [HttpGet]
        [RegexRoute("")]
        //[NpmPackageAccess("organisation", "package", Permissions.Read)]
        public async Task<IActionResult> GetPackage(string organisation, string package) {
            var identifier = new PackageIdentifier(organisation, CleanPackageName(package));
            var existing = await _packageRepository.GetAsync(identifier);

            if (existing == null)
                return PackageNotFoundResponse;

            return new JsonResult(new PackageViewModel() {
                Name = existing.Name,
                Revision = existing.Revision,
                DistTags = existing.DistTags,
                Versions = new Dictionary<string, Manifest>(existing.Versions.Where(kvp => kvp.Value != null))
            });
        }

        [HttpGet]
        [RegexRoute(TarballRoute)]
        //[NpmPackageAccess("organisation", "package", Permissions.Read)]
        public async Task<IActionResult> GetTarball(string organisation, string package, string version) {
            var identifier = new PackageVersionIdentifier(organisation, CleanPackageName(package), version);
            var tarball = await _tarballRepository.GetAsync(identifier);

            if (tarball == null)
                return PackageNotFoundResponse;

            return File(tarball.Data, "application/octet-stream");
        }

        [HttpDelete]
        [RegexRoute(TarballRoute + "/" + RevisionRoute)]
        //[NpmPackageAccess("organisation", "package", Permissions.Delete)]
        public IActionResult DeleteTarball() {
            // no-op here as we don't want to actually delete the data.
            return Ok("removed tarball");
        }

        [HttpPut]
        [RegexRoute("")]
        //[NpmPackageAccess("organisation", "package", Permissions.Create | Permissions.Update)]
        public async Task<IActionResult> UpdatePackage(string organisation, string package, [FromBody] UpdatePackageViewModel vm) {
            if (!ModelState.IsValid || CleanPackageName(package) != vm.Name)
                return BadRequestResponse;

            var identifier = new PackageIdentifier(organisation, vm.Name);
            var existing = await _packageRepository.GetAsync(identifier);

            if (existing == null)
                return PackageNotFoundResponse;

            return await UpdatePackageAsync(existing, vm, false);
        }

        [HttpDelete]
        [RegexRoute(RevisionRoute)]
        //[NpmPackageAccess("organisation", "package", Permissions.Create | Permissions.Update | Permissions.Delete)]
        public async Task<IActionResult> UnpublishPackageAsync(string organisation, string package, string revision) {
            var identifier = new PackageIdentifier(organisation, CleanPackageName(package));
            var existing = await _packageRepository.GetAsync(identifier);

            if (existing?.Revision != revision)
                return Error("conflict detected", HttpStatusCode.Conflict);

            return await UpdatePackageAsync(existing, new UpdatePackageViewModel() {
                DistTags = new Dictionary<string, string>(),
                Versions = new Dictionary<string, Manifest>()
            }, true);
        }

        [HttpPut]
        [RegexRoute(RevisionRoute)]
        //[NpmPackageAccess("organisation", "package", Permissions.Create | Permissions.Update | Permissions.Delete)]
        public async Task<IActionResult> UpdatePackageAsync(string organisation, string package, string revision, [FromBody]UpdatePackageViewModel vm) {
            if (!ModelState.IsValid || CleanPackageName(package) != vm.Name)
                return BadRequestResponse;

            var identifier = new PackageIdentifier(organisation, vm.Name);
            var existing = await _packageRepository.GetAsync(identifier);

            if (existing?.Revision != revision)
                return Error("conflict detected", HttpStatusCode.Conflict);

            return await UpdatePackageAsync(existing, vm, true);
        }

        private async Task<IActionResult> UpdatePackageAsync(Package package, UpdatePackageViewModel updates, bool deleteMissingVersions) {
            var tarballs = new List<Tarball>();

            package.DistTags = updates.DistTags;

            foreach (var kvp in updates.Versions) {
                var id = new PackageVersionIdentifier(package.Organisation, package.Name, kvp.Key);

                if (!updates.TryGetAttachment(id.Version, out var attachment)) {
                    if (!package.Versions.TryGetValue(id.Version, out var current) || current == null)
                        return Error($"cannot find version to update: ${id.Version}", HttpStatusCode.BadRequest);

                    // Update only select properties
                    current.Deprecated = kvp.Value.Deprecated;
                }
                else {
                    if (package.Versions.ContainsKey(id.Version))
                        return Error($"cannot overwrite existing version: {id.Version}", HttpStatusCode.BadRequest);

                    package.Versions.Add(kvp);

                    tarballs.Add(new Tarball() {
                        Data = Convert.FromBase64String(attachment.Data),
                        Version = id
                    });
                }
            }

            if (deleteMissingVersions) {
                var removedVersions = package.Versions.Keys
                    .Where(k => !updates.Versions.ContainsKey(k))
                    .ToList();

                foreach (var key in removedVersions)
                    package.Versions[key] = null;
            }

            await _tarballRepository.SaveAsync(tarballs).ConfigureAwait(false);

            try {
                var rev = await _packageRepository.UpdateAsync(package).ConfigureAwait(false);
                return Ok(rev);
            }
            catch {
                // Attempt to nuke the tarballs.
                try {
                    await _tarballRepository.DeleteAsync(tarballs.Select(t => t.Version)).ConfigureAwait(false);
                }
                catch {
                    /* nothing else we can do */
                }

                throw;
            }
        }

        private static string CleanPackageName(string name) {
            return name.Replace("%2f", "/", StringComparison.OrdinalIgnoreCase)
                .Replace("%40", "@");
        }
    }
}
