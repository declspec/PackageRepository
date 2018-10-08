using System;
using System.Collections.Generic;

namespace GetPkg.Auth.V2.Sso.OAuth2 {
    public class OAuth2AuthorizationConfiguration : ISsoAuthorizationConfiguration {
        public SsoProviderType Type => SsoProviderType.OAuth2;
        public Uri AuthorizationEndpoint { get; set; }
        public Uri TokenEndpoint { get; set; }

        public IUserProfileSourceConfiguration Profile { get; set; }

        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public IList<string> Scopes { get; set; }
    }
}
