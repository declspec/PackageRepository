using System;

namespace GetPkg.Auth {
    public interface ISsoAuthorizationOptions {
        string State { get; }
        Uri RedirectUri { get; }
    }

    public class SsoAuthorizationOptions : ISsoAuthorizationOptions {
        public string State { get; set; }
        public Uri RedirectUri { get; set; }
    }
}
