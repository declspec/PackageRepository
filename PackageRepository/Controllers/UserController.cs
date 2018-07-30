using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace PackageRepository.Controllers {
    [Route("")]
    public class UserController : ControllerBase {
        [HttpPut("-/user/{username}")]
        public IActionResult Login() {
            return new JsonResult(new { ok = true, token = "faketaxi" });
        }
    }
}
