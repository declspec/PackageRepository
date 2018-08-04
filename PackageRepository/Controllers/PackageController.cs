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
            if (!ModelState.IsValid || NormalizePackageName(package) != vm.Name)
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

                foreach (var kvp in vm.Attachments) {
                    var identifer = new PackageIdentifier(vm.Name, kvp.Key);
                    var manifest = vm.Versions[identifer.Version];

                    versions.Add(new PublishedPackageVersion() {
                        Version = new PackageVersion() {
                            Id = identifer,
                            Manifest = manifest
                        },
                        Tarball = new Tarball() {
                            Package = identifer,
                            Data = Convert.FromBase64String(kvp.Value.Data)
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
            var overview = await _packageService.GetPackageAsync(NormalizePackageName(package));

            if (overview == null)
                return NotFoundResponse;

            return new ObjectResult(new PackageViewModel() {
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
       
        private static string NormalizePackageName(string package) {
            return package.Replace("%2F", "/", StringComparison.OrdinalIgnoreCase)
                .Replace("%40", "@", StringComparison.Ordinal);
        }
    }
}
