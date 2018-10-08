using System;
using System.Collections.Generic;

namespace GetPkg.Auth.OAuth2 {
    public interface IOAuth2Configuration {
        Uri AuthorizationEndpoint { get; }
        Uri TokenEndpoint { get; }

        IUserProfileSourceConfiguration Profile { get; }

        string ClientId { get; }
        string ClientSecret { get; }
        IList<string> Scopes { get; }
    }

    public class OAuth2Configuration : IOAuth2Configuration {
        public Uri AuthorizationEndpoint { get; set; }
        public Uri TokenEndpoint { get; set; }

        public IUserProfileSourceConfiguration Profile { get; set; }

        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public IList<string> Scopes { get; set; }
    }
}
