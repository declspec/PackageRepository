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
using PackageRepository.Utils;

namespace PackageRepository.Controllers {
    [RegexRoute(Patterns.PackageName)]
    public class PackageController : ControllerBase {
        private static readonly IActionResult NotFoundResponse = Error("package not found", HttpStatusCode.NotFound);
        private static readonly IActionResult BadRequestResponse = Error("invalid request", HttpStatusCode.BadRequest);

        private readonly IPackageService _packageService;

        public PackageController(IPackageService packageService) {
            _packageService = packageService;
        }

        [HttpPut]
        [RegexRoute("")]
        public async Task<IActionResult> UpdatePackage(string package, [FromBody] UpdatePackageViewModel vm) {
            if (!ModelState.IsValid || PackageUtils.UnescapeName(package) != vm.Name)
                return BadRequestResponse;

            await _packageService.CommitAsync(vm.Name, ToChangeset(vm));
            await _packageService.SetDistTagsAsync(vm.Name, vm.DistTags);
            
            return Ok("updated package");
        }

        [HttpGet]
        [RegexRoute("")]
        public async Task<IActionResult> GetPackage(string package) {
            var overview = await _packageService.GetPackageAsync(PackageUtils.UnescapeName(package));

            if (overview == null)
                return NotFoundResponse;

            return new JsonResult(new PackageViewModel() {
                Name = overview.Name,
                DistTags = overview.DistTags,
                Versions = overview.Versions.ToDictionary(v => v.Id.Version, v => v.Manifest)
            });
        }

        [HttpGet]
        [RegexRoute(@"-/\k<package>-" + Patterns.SemVer + @"\.tgz")]
        public async Task<IActionResult> GetTarball(string package, string version) {
            var identifier = new PackageIdentifier(package, version);
            var tarball = await _packageService.GetTarballAsync(identifier);

            if (tarball == null)
                return NotFoundResponse;

            return File(tarball.Data, "application/octet-stream");
        }

        [HttpDelete]
        [RegexRoute("-rev/(?<revision>.+)")]
        public async Task<IActionResult> UnpublishPackageAsync(string package, string revision) {
            var overview = await _packageService.GetPackageAsync(PackageUtils.UnescapeName(package));

            if (overview == null)
                return NotFoundResponse;

            var changeset = new PackageChangeset() {
                Deleted = overview.Versions.Select(v => v.Id).ToList()
            };

            await _packageService.CommitAsync(overview.Name, changeset);
            return Ok("removed all package versions");
        }

        [HttpPut]
        [RegexRoute("-rev/(?<revision>.+)")]
        public async Task<IActionResult> UpdatePackageAsync(string package, string revision, [FromBody]UpdatePackageViewModel vm) {
            if (!ModelState.IsValid || PackageUtils.UnescapeName(package) != vm.Name)
                return BadRequestResponse;

            var overview = await _packageService.GetPackageAsync(package);

            if (overview == null)
                return NotFoundResponse;

            var changeset = ToChangeset(vm);

            // Allow for deletes when a revision is specified (this is basically a 'replace the model' update)
            changeset.Deleted = overview.Versions.Select(v => v.Id)
                .Where(id => !vm.Versions.ContainsKey(id.Version))
                .ToList();

            await _packageService.CommitAsync(overview.Name, changeset);

            var results = new [] {
                Tuple.Create(changeset.Published.Count, "published"),
                Tuple.Create(changeset.Updated.Count, "updated"),
                Tuple.Create(changeset.Deleted.Count, "unpublished")
            };

            return Ok(string.Join(", ", results.Where(t => t.Item1 > 0).Select(t => $"{t.Item1} {t.Item2}")));
        }

        private static IActionResult Ok(string message) {
            return new JsonResult(new { ok = message });
        }

        private static IActionResult Error(string message, HttpStatusCode status) {
            return new JsonResult(new { error = message }) { StatusCode = (int)status };
        }

        private static PackageChangeset ToChangeset(UpdatePackageViewModel vm) {
            var changeset = new PackageChangeset() {
                Updated = new List<PackageVersion>(),
                Published = new List<PublishedPackageVersion>()
            };

            foreach (var kvp in vm.Versions) {
                var id = new PackageIdentifier(vm.Name, kvp.Key);

                if (vm.Attachments == null || !vm.Attachments.TryGetValue(PackageUtils.GetTarballName(id), out var attachment)) {
                    changeset.Updated.Add(new PackageVersion() {
                        Id = id,
                        Manifest = kvp.Value
                    });
                }
                else {
                    changeset.Published.Add(new PublishedPackageVersion() {
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
