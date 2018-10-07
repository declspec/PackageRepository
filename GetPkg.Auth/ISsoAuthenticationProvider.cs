using Fiksu.Web;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GetPkg.Auth {
    public interface ISsoAuthenticationProvider {
        Task<Uri> GetAuthorizationUriAsync(SsoRequestOptions options);
        Task<ClaimsPrincipal> AuthenticateAsync(IHttpRequest request, SsoRequestOptions originalOptions);
    }
}
