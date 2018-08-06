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
        private static readonly IActionResult BadRequestResponse = new StatusCodeResult((int)HttpStatusCode.BadRequest);
        private static readonly IActionResult NotFoundResponse = new StatusCodeResult((int)HttpStatusCode.NotFound);
        private static readonly IActionResult OkResponse = new StatusCodeResult((int)HttpStatusCode.OK);

        private readonly IPackageService _packageService;

        public PackageController(IPackageService packageService) {
            _packageService = packageService;
        }

        [HttpPut]
        [RegexRoute("")]
        public async Task<IActionResult> UpdatePackage(string package, [FromBody] UpdatePackageViewModel vm) {
            if (!ModelState.IsValid || PackageUtils.UnescapeName(package) != vm.Name)
                return BadRequestResponse;

            if (vm.Attachments == null) {
                await _packageService.UpdatePackageVersionsAsync(vm.Name, vm.Versions.Select(kvp => new PackageVersion() {
                    Id = new PackageIdentifier(vm.Name, kvp.Key),
                    Manifest = kvp.Value
                }));
            }
            else {
                // If there are attachments, assume it's a publish action (no support for a simultaneous publish+update as yet)
                var versions = new List<PublishedPackageVersion>();

                foreach(var kvp in vm.Versions) {
                    if (!vm.Attachments.TryGetValue(PackageUtils.GetTarballName(vm.Name, kvp.Key), out var attachment))
                        continue;

                    var identifer = new PackageIdentifier(vm.Name, kvp.Key);

                    versions.Add(new PublishedPackageVersion() {
                        Id = identifer,
                        Manifest = kvp.Value,
                        Tarball = new Tarball() {
                            Package = identifer,
                            Data = Convert.FromBase64String(attachment.Data)
                        }
                    });
                }

                // Run sequentially to ensure no errors occurred publishing packages before setting dist-tags
                await _packageService.PublishPackageVersionsAsync(versions);
                await _packageService.SetDistTagsAsync(vm.Name, vm.DistTags);
            }

            return OkResponse;
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

            await _packageService.UnpublishPackageVersionsAsync(overview.Versions.Select(v => v.Id));
            return Ok("unpublished whole package");
        }

        [HttpPut]
        [RegexRoute("-rev/(?<revision>.+)")]
        public async Task<IActionResult> UnpublishPackageAsync(string package, string revision, [FromBody]UpdatePackageViewModel vm) {
            if (!ModelState.IsValid || PackageUtils.UnescapeName(package) != vm.Name)
                return BadRequestResponse;

            var overview = await _packageService.GetPackageAsync(package);

            if (overview == null)
                return NotFoundResponse;

            var unpublished = overview.Versions.Where(version => !vm.Versions.ContainsKey(version.Id.Version)).ToList();
            await _packageService.UnpublishPackageVersionsAsync(unpublished.Select(v => v.Id));

            return Ok($"unublished {unpublished.Count} versions");
        }

        private static IActionResult Ok(string message) {
            return new JsonResult(new { ok = message });
        }

        private static IActionResult Error(string message, HttpStatusCode status) {
            return new JsonResult(new { error = message }) { StatusCode = (int)status };
        }
    }
}
