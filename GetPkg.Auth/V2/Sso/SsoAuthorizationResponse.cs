using System.Collections.Generic;

namespace GetPkg.Auth.V2.Sso {
    public interface ISsoAuthorizationResponse {
        IDictionary<string, string> Parameters { get; }
    }

    public class SsoAuthorizationResponse : ISsoAuthorizationResponse {
        public IDictionary<string, string> Parameters { get; }

        public SsoAuthorizationResponse(IDictionary<string, string> parameters) {
            Parameters = parameters;
        }
    }
}
