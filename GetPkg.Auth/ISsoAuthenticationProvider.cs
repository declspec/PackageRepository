using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GetPkg.Auth {
    public interface ISsoAuthenticationProvider {
        Task<Uri> GetAuthorizationUriAsync(ISsoAuthorizationOptions options);
        Task<IUserProfile> CompleteAuthorizationAsync(IDictionary<string, string> parameters, ISsoAuthorizationOptions options);
        Task<IUserProfile> RefreshAsync(string refreshToken);
    }
}
