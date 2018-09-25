using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace PackageRepository.Controllers.Npm {
    public abstract class NpmControllerBase : ControllerBase {
        protected static readonly IActionResult BadRequestResponse = Error("invalid request", HttpStatusCode.BadRequest);

        protected static IActionResult Ok(string message) {
            return new JsonResult(new { ok = message });
        }

        protected static IActionResult Error(string message, HttpStatusCode status) {
            return new JsonResult(new { error = message }) { StatusCode = (int)status };
        }
    }
}
