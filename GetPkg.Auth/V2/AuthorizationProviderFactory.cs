using System;
using System.Collections.Generic;
using System.Text;

namespace GetPkg.Auth.V2 {
    public interface IAuthorizationProviderFactory {
        ISsoAuthorizationProvider GetSsoProvider(SsoProviderType type);
        ICre
    }

    class AuthorizationProviderFactory {
    }
}
