using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GetPkg.Auth.Oidc {
    public class OidcAuthenticationProvider : ISsoAuthenticationProvider {
        public Task<IUserProfile> CompleteAuthorizationAsync(IDictionary<string, string> parameters, ISsoAuthorizationOptions options) {
            throw new NotImplementedException();
        }

        public Task<Uri> GetAuthorizationUriAsync(ISsoAuthorizationOptions options) {
            throw new NotImplementedException();
        }

        public Task<IUserProfile> RefreshAsync(string refreshToken) {
            throw new NotImplementedException();
        }
    }
}
