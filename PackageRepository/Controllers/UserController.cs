using Microsoft.AspNetCore.Mvc;

namespace PackageRepository.Controllers {
    [Route("{organisation}/npm")]
    public class UserController : ControllerBase {
        [HttpPut("-/user/{username}")]
        public IActionResult Login(string username) {
            return new JsonResult(new { ok = true, token = "faketaxi" });
        }
    }
}
