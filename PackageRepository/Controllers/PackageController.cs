using FiksuCore.Web.Routing;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PackageRepository.Constants;
using PackageRepository.Models;
using PackageRepository.Services;
using PackageRepository.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace PackageRepository.Controllers {
    [RegexRoute(Patterns.PackageName)]
    public class PackageController : ControllerBase {
        private static readonly IActionResult BadRequestResponse = new StatusCodeResult((int)HttpStatusCode.BadRequest);
        private static readonly IActionResult NotFoundResponse = new StatusCodeResult((int)HttpStatusCode.NotFound);
        private static readonly IActionResult OkResponse = new StatusCodeResult((int)HttpStatusCode.OK);

        private static readonly Task<IActionResult> BadRequestResponseTask = Task.FromResult(BadRequestResponse);
        private static readonly Task<IActionResult> NotFoundResponseTask = Task.FromResult(NotFoundResponse);

        private static readonly JsonSerializerSettings DefaultSerializerSettings = new JsonSerializerSettings() {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver() {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };

        private readonly IPackageService _packageService;

        public PackageController(IPackageService packageService) {
            _packageService = packageService;
        }

        [HttpPut]
        [RegexRoute("")]
        public Task<IActionResult> UpdatePackage(string package, UpdatePackageViewModel vm) {
            return BadRequestResponseTask;
        }

        [HttpGet]
        [RegexRoute("")]
        public async Task<IActionResult> GetPackage(string package) {
            var overview = await _packageService.GetPackageAsync(NormalizePackageName(package));

            if (overview == null)
                return new NotFoundResult();

            var sw = new StreamWriter(new MemoryStream());

            try {
                using (var writer = new JsonTextWriter(sw)) {
                    writer.CloseOutput = false;

                    writer.WriteStartObject();
                    writer.WritePropertyName("name");
                    writer.WriteValue(package);
                    writer.WritePropertyName("dist-tags");
                    writer.WriteRawValue(JsonConvert.SerializeObject(overview.DistTags));
                    writer.WritePropertyName("versions");
                    writer.WriteStartObject();

                    foreach (var version in overview.Versions) {
                        writer.WritePropertyName(version.Id.Version);
                        writer.WriteRawValue(version.Manifest);
                    }

                    writer.WriteEndObject();
                    writer.WriteEndObject();
                    writer.Flush();
                }
                
                sw.BaseStream.Position = 0;
                return new FileStreamResult(sw.BaseStream, "application/json; charset=utf-8");
            }
            catch(Exception) {
                if (sw != null)
                    sw.Dispose();
                throw;
            }
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

        public async Task<IActionResult> UpdatePackageAsync(string package, UpdatePackageViewModel viewModel) {
            if (!ModelState.IsValid || viewModel.Versions.Count == 0 || package != viewModel.Name)
                return BadRequestResponse;

            if (viewModel.Attachments == null)
                return BadRequestResponse;
            else {
                // If there are attachments, assume it's a publish action (no support for a simultaneous publish+update as yet)
                var versions = new List<PublishedPackageVersion>();

                foreach(var kvp in viewModel.Attachments) {
                    var identifer = new PackageIdentifier(package, kvp.Key);

                    if (!viewModel.Versions.TryGetValue(identifer.Version, out var manifest))
                        return BadRequestResponse;

                    versions.Add(new PublishedPackageVersion() {
                        Version = new PackageVersion() {
                            Id = identifer,
                            Manifest = JsonConvert.SerializeObject(manifest, DefaultSerializerSettings)
                        },
                        Tarball = new Tarball() {
                            Package = identifer,
                            Data = Convert.FromBase64String(kvp.Value.Data)
                        }
                    });
                }

                await _packageService.PublishPackageVersionsAsync(versions);
            }

            return OkResponse;
        }

        private Task<IActionResult> GetTarballAsync(string package, string version) {
            return BadRequestResponseTask;
            //var tarball = await _tarballRepository.GetByPackageVersionAsync(package, version);
            //return new FileContentResult(tarball.Data, "application/octet-stream");
        }

        private static string NormalizePackageName(string package) {
            return package.Replace("%2F", "/", StringComparison.OrdinalIgnoreCase)
                .Replace("%40", "@", StringComparison.Ordinal);
        }
    }
}
