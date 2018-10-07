using System;
using System.Collections.Generic;

namespace GetPkg.Auth.OAuth2 {
    public class OAuth2Configuration {
        public Uri AuthorizationEndpoint { get; set; }
        public Uri TokenEndpoint { get; set; }

        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public IList<string> Scopes { get; set; }
    }
}
