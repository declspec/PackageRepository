using FiksuCore.Web.Http.Extensions;
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
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PackageRepository.Controllers {
    [RegexRoute(Patterns.PackageName)]
    public class PackageController : ControllerBase {
        private static readonly IActionResult BadRequestResponse = new StatusCodeResult((int)HttpStatusCode.BadRequest);
        private static readonly Task<IActionResult> BadRequestResponseTask = Task.FromResult(BadRequestResponse);

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

            using (var ms = new MemoryStream()) {
                using (var sw = new StreamWriter(ms))
                using (var jsonWriter = new JsonTextWriter(sw)) {
                    jsonWriter.WriteStartObject();
                    jsonWriter.WritePropertyName("name");
                    jsonWriter.WriteValue(package);
                    jsonWriter.WritePropertyName("dist-tags");
                    jsonWriter.WriteRawValue(JsonConvert.SerializeObject(overview.DistTags));
                    jsonWriter.WritePropertyName("versions");
                    jsonWriter.WriteStartObject();

                    foreach (var version in overview.Versions) {
                        jsonWriter.WritePropertyName(version.Id.Version);
                        jsonWriter.WriteRawValue(version.Manifest);
                    }

                    jsonWriter.WriteEndObject();
                    jsonWriter.WriteEndObject();
                }

                return new FileStreamResult(ms, "application/json; charset=utf-8");
            }
        }

        [HttpGet]
        [RegexRoute(@"-/\k<package>-" + Patterns.SemVer + @"\.tgz")]
        public Task<IActionResult> GetTarball(string package, string version) {
            return BadRequestResponseTask;
            //var tarball = await _tarballRepository.GetByPackageVersionAsync(package, version);
            //return new FileContentResult(tarball.Data, "application/octet-stream");
        }

        public async Task<IActionResult> UpdatePackageAsync(string package, UpdatePackageViewModel viewModel) {
            if (!ModelState.IsValid || viewModel.Versions.Count == 0 || package != viewModel.Name)
                return BadRequestResponse;

            var tasks = new List<Task>();

            foreach(var kvp in viewModel.Versions) {
                var version = new PackageVersion() {
                    Id = new PackageIdentifier(NormalizePackageName(package), kvp.Key),
                    Manifest = JsonConvert.SerializeObject(kvp.Value, DefaultSerializerSettings)
                };

                // Assume a publish if there
                if (viewModel.Attachments == null || !viewModel.Attachments.TryGetValue(version.Id.Version, out var attachment))
                    tasks.Add(_packageService.UpdatePackageVersionAsync(version));
                else { 
                    tasks.Add(_packageService.PublishPackageVersionAsync(new PublishedPackageVersion() {
                        Version = version,
                        Tarball = new Tarball() {
                            Package = version.Id,
                            Data = Convert.FromBase64String(attachment.Data)
                        }
                    }));
                }
            }

            if (tasks.Count == 0)
                return BadRequestResponse;

            await Task.WhenAll(tasks);
            return Response.Ok(null);
        }

        private Task<IActionResult> GetTarballAsync(string package, string version) {
            return BadRequestResponseTask;
            //var tarball = await _tarballRepository.GetByPackageVersionAsync(package, version);
            //return new FileContentResult(tarball.Data, "application/octet-stream");
        }

        private static string NormalizePackageName(string package) {
            return package.Replace("%2F", "/", StringComparison.OrdinalIgnoreCase);
        }
    }
}
