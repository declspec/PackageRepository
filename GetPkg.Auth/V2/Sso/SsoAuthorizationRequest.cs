using System;

namespace GetPkg.Auth.V2 {
    public interface ISsoAuthorizationRequest {
        string State { get; }
        Uri RedirectUri { get; }
    }

    public class SsoAuthorizationRequest : ISsoAuthorizationRequest {
        public string State { get; }
        public Uri RedirectUri { get; }

        public SsoAuthorizationRequest(string state, Uri redirectUri) {
            State = state;
            RedirectUri = redirectUri;
        }
    }
}
