using FiksuCore.Web.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PackageRepository.Models;
using PackageRepository.Services;
using PackageRepository.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PackageRepository.Controllers {
    [Route("")]
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

        [HttpPut("-/user/{username}")]
        public IActionResult Login() {
            return new JsonResult(new { ok = true, token = "faketaxi" });
        }

        [HttpPut("@{scope:packagename}%2f{package:packagename}")]
        public Task<IActionResult> PublishEscapedScopedPackage(string scope, string package, [FromBody]CreatePackageVersionViewModel viewModel) {
            return PublishPackageAsync($"@{scope}/{package}", viewModel);
        }

        [HttpPut("@{scope:packagename}/{package:packagename}")]
        public Task<IActionResult> PublishScopedPackage(string scope, string package, [FromBody]CreatePackageVersionViewModel viewModel) {
            return PublishPackageAsync($"@{scope}/{package}", viewModel);
        }

        [HttpPut("{package:packagename}")]
        public Task<IActionResult> PublishPackage(string package, [FromBody]CreatePackageVersionViewModel viewModel) {
            return PublishPackageAsync(package, viewModel);
        }

        [HttpGet("@{scope:packagename}%2f{package:packagename}")]
        public Task<IActionResult> GetEscapedScopedPackage(string scope, string package) {
            return GetPackage($"@{scope}/{package}");
        }

        [HttpGet("@{scope:packagename}/{package:packagename}")]
        public Task<IActionResult> GetScopedPackage(string scope, string package) {
            return GetPackage($"@{scope}/{package}");
        }

        [HttpGet("{package:packagename}")]
        public Task<IActionResult> GetPackage(string package) {
            return GetPackageAsync(package);
        }

        [HttpGet("@{scope:packagename}/{package:packagename}/-/@{scope2:packagename}/{package2:packagename}-{version:semver}.tgz")]
        public Task<IActionResult> GetScopedTarball(string scope, string package, string scope2, string package2, string version) {
            return (scope == scope2 && package == package2) 
                ? GetTarballAsync($"@{scope}/{package}", version) 
                : BadRequestResponseTask;
        }

        [HttpGet("{package:packagename}/-/{package2:packagename}-{version:semver}.tgz")]
        public Task<IActionResult> GetTarball(string package, string package2, string version) {
            return package == package2 
                ? GetTarballAsync(package, version) 
                : BadRequestResponseTask;
        }

        public async Task<IActionResult> PublishPackageAsync(string package, CreatePackageVersionViewModel viewModel) {
            if (!ModelState.IsValid || viewModel.Versions.Count != 1 || viewModel.Attachments.Count != 1)
                return BadRequestResponse;

            var version = viewModel.Versions.Keys.First();
            var attachment = viewModel.Attachments.Keys.First();
            var identifier = new PackageIdentifier(viewModel.Name, version);

            var packageModel = new PackageVersion() {
                Id = identifier,
                Manifest = JsonConvert.SerializeObject(viewModel.Versions[version], DefaultSerializerSettings)
            };

            var tarballModel = new Tarball() {
                Package = identifier,
                Data = Convert.FromBase64String(viewModel.Attachments[attachment].Data)
            };

            await _packageService.PublishPackageAsync(new PublishedPackage() {
                Version = packageModel,
                Tarball = tarballModel,
                DistTags = viewModel.DistTags
            });

            return Response.Created(null);
        }

        public async Task<IActionResult> GetPackageAsync(string package) {
            var overview = await _packageService.GetPackageOverviewAsync(package);

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

                return new FileContentResult(ms.ToArray(), "application/json; charset=utf-8");
            }
        }

        private Task<IActionResult> GetTarballAsync(string package, string version) {
            return BadRequestResponseTask;
            //var tarball = await _tarballRepository.GetByPackageVersionAsync(package, version);
            //return new FileContentResult(tarball.Data, "application/octet-stream");
        }
    }
}
