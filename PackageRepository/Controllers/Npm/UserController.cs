using System;
using System.Threading.Tasks;
using FiksuCore.Web.Routing;
using GetPkg;
using Microsoft.AspNetCore.Mvc;
using PackageRepository.Constants;
using PackageRepository.ViewModels.Npm;

namespace PackageRepository.Controllers.Npm {
    [RegexRoute(Patterns.OrganisationName + "/npm/-/user")]
    public class UserController : NpmControllerBase {
        //private readonly ITokenRepository _tokenRepository;

        //public UserController(ITokenRepository tokenRepository) {
        //    _tokenRepository = tokenRepository;
        //}

        [HttpPut]
        [RegexRoute(@"org\.couchdb\.user:npm_(?:oauth|saml)_auth_dummy_user")]
        public async Task<IActionResult> SsoLogin(string organisation) {
            var token = await Task.FromResult(new Token() { Id = Guid.NewGuid().ToString() });
            var ssoUrl = $"https://github.curtin.edu.au/login/oauth/authorize?client_id=cec4accd758f6c85a888&client_secret=795516cf170e9bb0ccbe4a0243bdac5cde5a3dc4&state={token.Id}&redirect_uri=http://localhost:5000/authorize&scope=public_repo%20read:org%20user:email";
            return new JsonResult(new { token = token.Id, sso = ssoUrl }) { StatusCode = 201 };
        }

        [HttpPut]
        [RegexRoute("(?<username>" + Patterns.PathSegment + ")")]
        public IActionResult Login(string organisation, string username, [FromBody] LoginViewModel vm) {
            if (!ModelState.IsValid || username != vm.Id)
                return BadRequestResponse;

            return Ok(new { token = "faketaxi" });
        }

        [HttpGet("authorize")]
        public IActionResult Authorize() {
            return Ok(new { token = "" });
        }
    }
}
