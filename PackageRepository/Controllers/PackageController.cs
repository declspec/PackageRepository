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
using PackageRepository.Database.Repositories;

namespace PackageRepository.Controllers {
    [RegexRoute(Patterns.OrganisationName + "/npm/" + Patterns.PackageName)]
    public class PackageController : ControllerBase {
        private const string TarballRoute = @"-/\k<package>-" + Patterns.SemVer + @"\.tgz";
        private const string RevisionRoute = "-rev/(?<revision>.+)";

        private static readonly IActionResult PackageNotFoundResponse = Error("package not found", HttpStatusCode.NotFound);
        private static readonly IActionResult BadRequestResponse = Error("invalid request", HttpStatusCode.BadRequest);
        private static readonly IActionResult DuplicateVersionResponse = Error("cannot overwrite existing package version", HttpStatusCode.NotAcceptable);

        private readonly IPackageService _packageService;
        private readonly IThingRepository _thingRepository;

        public PackageController(IPackageService packageService, IThingRepository thingRepository) {
            _packageService = packageService;
            _thingRepository = thingRepository;
        }

        [HttpGet]
        [RegexRoute("permissions")]
        public async Task<IActionResult> GetPackagePermissions(string organisation, string package) {
            var identifier = new ThingIdentifier(organisation, Enums.ThingType.NpmPackage, PackageUtils.UnescapeName(package));
            var thing = await _thingRepository.GetThingAsync(identifier);

            if (thing == null)
                return PackageNotFoundResponse;

            var permissions = await _thingRepository.GetThingPermissionsAsync(thing.Id);
            return new JsonResult(permissions);
        }

        [HttpPut]
        [RegexRoute("")]
        public async Task<IActionResult> UpdatePackage(string package, [FromBody] UpdatePackageViewModel vm) {
            if (!ModelState.IsValid || PackageUtils.UnescapeName(package) != vm.Name)
                return BadRequestResponse;

            try {
                await _packageService.CommitAsync(vm.Name, ToPatch(vm));
                return Ok("updated package");
            }
            catch (DuplicatePackageVersionException) {
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

            var patch = new PackagePatch() {
                DeletedVersions = overview.Versions.Select(v => v.Id).ToList()
            };

            await _packageService.CommitAsync(overview.Name, patch);
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

            var patch = ToPatch(vm);

            // Allow for deletes when a revision is specified (this is basically a 'replace the model' update)
            patch.DeletedVersions = overview.Versions.Select(v => v.Id)
                .Where(id => !vm.Versions.ContainsKey(id.Version))
                .ToList();

            try {
                await _packageService.CommitAsync(overview.Name, patch);
                return Ok(GetResults(patch));
            }
            catch (DuplicatePackageVersionException) {
                return DuplicateVersionResponse;
            }
            catch (PackageVersionNotFoundException nfe) {
                return Error($"package version not found ({nfe.Identifier.Version})", HttpStatusCode.BadRequest);
            }
        }

        private static IActionResult Ok(string message) {
            return new JsonResult(new { ok = message });
        }

        private static IActionResult Error(string message, HttpStatusCode status) {
            return new JsonResult(new { error = message }) { StatusCode = (int)status };
        }

        private static string GetResults(IPackagePatch patch) {
            var results = new[] {
                Tuple.Create(patch.PublishedVersions.Count, "published"),
                Tuple.Create(patch.UpdatedVersions.Count, "updated"),
                Tuple.Create(patch.DeletedVersions.Count, "unpublished")
            };

            return string.Join(", ", results.Where(t => t.Item1 > 0).Select(t => $"{t.Item1} {t.Item2}"));
        }

        private static PackagePatch ToPatch(UpdatePackageViewModel vm) {
            var patch = new PackagePatch() {
                UpdatedVersions = new List<PackageVersion>(),
                PublishedVersions = new List<PublishedPackageVersion>(),
                UpdatedDistTags = vm.DistTags
            };

            foreach (var kvp in vm.Versions) {
                var id = new PackageIdentifier(vm.Name, kvp.Key);

                var version = new PublishedPackageVersion() {
                    Id = id,
                    Manifest = kvp.Value
                };

                if (vm.Attachments == null || !vm.Attachments.TryGetValue(PackageUtils.GetTarballName(id), out var attachment))
                    patch.UpdatedVersions.Add(version);
                else {
                    version.Tarball = new Tarball() {
                        Package = id,
                        Data = Convert.FromBase64String(attachment.Data)
                    };

                    patch.PublishedVersions.Add(version);
                }
            }

            return patch;
        }
    }
}
