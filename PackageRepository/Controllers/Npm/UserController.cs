using System;
using System.Threading.Tasks;
using Fiksu.Web;
using FiksuCore.Web.Routing;
using GetPkg;
using GetPkg.Auth;
using GetPkg.Auth.OAuth2;
using Microsoft.AspNetCore.Mvc;
using PackageRepository.Constants;
using PackageRepository.ViewModels.Npm;

namespace PackageRepository.Controllers.Npm {
    [RegexRoute(Patterns.OrganisationName + "/npm/-/user")]
    public class UserController : NpmControllerBase {
        private static readonly OAuth2Configuration GitHubConfiguration = new OAuth2Configuration() {
            AuthorizationEndpoint = new Uri("https://github.curtin.edu.au/login/oauth/authorize"),
            TokenEndpoint = new Uri("https://github.curtin.edu.au/login/oauth/access_token"),
            ClientId = "cec4accd758f6c85a888",
            ClientSecret = "795516cf170e9bb0ccbe4a0243bdac5cde5a3dc4",
            Scopes = new[] { "public_repo", "read:org", "user:email" }
        };

        private static readonly SsoRequestOptions StaticRequestOptions = new SsoRequestOptions() {
            State = "dumb.state",
            RedirectUri = new Uri("http://localhost:5000/authorize")
        };

        private static readonly ISsoAuthenticationProvider SsoProvider = new OAuth2AuthenticationProvider(GitHubConfiguration, new System.Net.Http.HttpClient());

        //private readonly ITokenRepository _tokenRepository;

        //public UserController(ITokenRepository tokenRepository) {
        //    _tokenRepository = tokenRepository;
        //}

        [HttpPut]
        [RegexRoute(@"org\.couchdb\.user:npm_(?:oauth|saml)_auth_dummy_user")]
        public async Task<IActionResult> SsoLogin(string organisation) {
            var token = await Task.FromResult(new Token() { Id = Guid.NewGuid().ToString() });
            var redirectUri = await SsoProvider.GetAuthorizationUriAsync(StaticRequestOptions);

            return new JsonResult(new { token = token.Id, sso = redirectUri }) { StatusCode = 201 };
        }

        [HttpPut]
        [RegexRoute("(?<username>" + Patterns.PathSegment + ")")]
        public IActionResult Login(string organisation, string username, [FromBody] LoginViewModel vm) {
            if (!ModelState.IsValid || username != vm.Id)
                return BadRequestResponse;

            return Ok(new { token = "faketaxi" });
        }

        [HttpGet("authorize")]
        public async Task<IActionResult> Authenticate() {
            var principal = await SsoProvider.AuthenticateAsync(HttpContext.Features.Get<IHttpContext>().Request, StaticRequestOptions);
            return Ok(new { token = "" });
        }
    }
}
