using FiksuCore.Web.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using PackageRepository.Database.Repositories;
using PackageRepository.Models;
using PackageRepository.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace PackageRepository.Controllers {
    [Route("")]
    public class PackageController : ControllerBase {
        private readonly IPackageRepository _packageRepository;

        public PackageController(IPackageRepository packageRepository) {
            _packageRepository = packageRepository;
        }

        [HttpPut("-/user/{username}")]
        public IActionResult Login() {
            return new JsonResult(new { ok = true, token = "faketaxi" });
        }

        [HttpPut("{package}")]
        public async Task<IActionResult> PublishPackage(string package, [FromBody]CreatePackageVersionViewModel viewModel) {
            if (!ModelState.IsValid || viewModel.Versions.Count != 1)
                return Response.ValidationError();

            var version = viewModel.Versions.Keys.First();

            var model = new CreatePackageVersion() {
                Name = viewModel.Name,
                Version = version,
                DistTags = viewModel.DistTags,
                Manifest = viewModel.Versions[version]
            };

            await _packageRepository.CreateVersionAsync(model);
            return Response.Created(null);
        }
    }
}
