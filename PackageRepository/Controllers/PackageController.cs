using FiksuCore.Web.Routing;
using Microsoft.AspNetCore.Mvc;
using PackageRepository.Constants;
using PackageRepository.Models;
using PackageRepository.Services;
using PackageRepository.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using PackageRepository.Exceptions;
using PackageRepository.Utils;

namespace PackageRepository.Controllers {
    [RegexRoute(Patterns.OrganisationName + "/npm/" + Patterns.PackageName)]
    public class PackageController : ControllerBase {
        private const string TarballRoute = @"-/\k<package>-" + Patterns.SemVer + @"\.tgz";
        private const string RevisionRoute = "-rev/(?<revision>.+)";

        private static readonly IActionResult PackageNotFoundResponse = Error("package not found", HttpStatusCode.NotFound);
        private static readonly IActionResult BadRequestResponse = Error("invalid request", HttpStatusCode.BadRequest);
        private static readonly IActionResult DuplicateVersionResponse = Error("cannot overwrite existing package version", HttpStatusCode.NotAcceptable);

        private readonly IPackageService _packageService;

        public PackageController(IPackageService packageService) {
            _packageService = packageService;
        }

        [HttpPut]
        [RegexRoute("")]
        public async Task<IActionResult> UpdatePackage(string package, [FromBody] UpdatePackageViewModel vm) {
            if (!ModelState.IsValid || PackageUtils.UnescapeName(package) != vm.Name)
                return BadRequestResponse;

            try {
                await _packageService.CommitAsync(vm.Name, ToChangeset(vm));
                return Ok("updated package");
            }
            catch(DuplicatePackageVersionException) {
                return DuplicateVersionResponse;
            }
            catch (PackageVersionNotFoundException nfe) {
                return Error($"package version not found ({nfe.Identifier.Version})", HttpStatusCode.BadRequest);
            }
        }

        [HttpGet]
        [RegexRoute("")]
        public async Task<IActionResult> GetPackage(string package) {
            var overview = await _packageService.GetPackageAsync(PackageUtils.UnescapeName(package));

            if (overview == null)
                return PackageNotFoundResponse;

            return new JsonResult(new PackageViewModel() {
                Name = overview.Name,
                DistTags = overview.DistTags,
                Versions = overview.Versions.ToDictionary(v => v.Id.Version, v => v.Manifest)
            });
        }

        [HttpGet]
        [RegexRoute(TarballRoute)]
        public async Task<IActionResult> GetTarball(string package, string version) {
            var identifier = new PackageIdentifier(package, version);
            var tarball = await _packageService.GetTarballAsync(identifier);

            if (tarball == null)
                return PackageNotFoundResponse;

            return File(tarball.Data, "application/octet-stream");
        }

        [HttpDelete]
        [RegexRoute(TarballRoute + "/" + RevisionRoute)]
        public IActionResult DeleteTarball() {
            // no-op here as we don't want to actually delete the data.
            return Ok("removed tarball");
        }

        [HttpDelete]
        [RegexRoute(RevisionRoute)]
        public async Task<IActionResult> UnpublishPackageAsync(string package, string revision) {
            var overview = await _packageService.GetPackageAsync(PackageUtils.UnescapeName(package));

            if (overview == null)
                return PackageNotFoundResponse;

            var changeset = new PackageChangeset() {
                DeletedVersions = overview.Versions.Select(v => v.Id).ToList()
            };
            
            await _packageService.CommitAsync(overview.Name, changeset);
            return Ok("removed all package versions");
        }

        [HttpPut]
        [RegexRoute(RevisionRoute)]
        public async Task<IActionResult> UpdatePackageAsync(string package, string revision, [FromBody]UpdatePackageViewModel vm) {
            if (!ModelState.IsValid || PackageUtils.UnescapeName(package) != vm.Name)
                return BadRequestResponse;

            var overview = await _packageService.GetPackageAsync(vm.Name);

            if (overview == null)
                return PackageNotFoundResponse;

            var changeset = ToChangeset(vm);

            // Allow for deletes when a revision is specified (this is basically a 'replace the model' update)
            changeset.DeletedVersions = overview.Versions.Select(v => v.Id)
                .Where(id => !vm.Versions.ContainsKey(id.Version))
                .ToList();

            try {
                await _packageService.CommitAsync(overview.Name, changeset);
                return Ok(GetResults(changeset));
            }
            catch(DuplicatePackageVersionException) {
                return DuplicateVersionResponse;
            }
            catch(PackageVersionNotFoundException nfe) {
                return Error($"package version not found ({nfe.Identifier.Version})", HttpStatusCode.BadRequest);
            }
        }

        private static IActionResult Ok(string message) {
            return new JsonResult(new { ok = message });
        }

        private static IActionResult Error(string message, HttpStatusCode status) {
            return new JsonResult(new { error = message }) { StatusCode = (int)status };
        }

        private static string GetResults(IPackageChangeset changeset) {
            var results = new[] {
                Tuple.Create(changeset.PublishedVersions.Count, "published"),
                Tuple.Create(changeset.UpdatedVersions.Count, "updated"),
                Tuple.Create(changeset.DeletedVersions.Count, "unpublished")
            };

            return string.Join(", ", results.Where(t => t.Item1 > 0).Select(t => $"{t.Item1} {t.Item2}"));
        }

        private static PackageChangeset ToChangeset(UpdatePackageViewModel vm) {
            var changeset = new PackageChangeset() {
                UpdatedVersions = new List<PackageVersion>(),
                PublishedVersions = new List<PublishedPackageVersion>(),
                UpdatedDistTags = vm.DistTags
            };

            foreach (var kvp in vm.Versions) {
                var id = new PackageIdentifier(vm.Name, kvp.Key);

                if (vm.Attachments == null || !vm.Attachments.TryGetValue(PackageUtils.GetTarballName(id), out var attachment)) {
                    changeset.UpdatedVersions.Add(new PackageVersion() {
                        Id = id,
                        Manifest = kvp.Value
                    });
                }
                else {
                    changeset.PublishedVersions.Add(new PublishedPackageVersion() {
                        Id = id,
                        Manifest = kvp.Value,
                        Tarball = new Tarball() {
                            Package = id,
                            Data = Convert.FromBase64String(attachment.Data)
                        }
                    });
                }
            }

            return changeset;
        }
    }
}
