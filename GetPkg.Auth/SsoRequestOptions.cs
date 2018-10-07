using System;

namespace GetPkg.Auth {
    public class SsoRequestOptions {
        public string State { get; set; }
        public Uri RedirectUri { get; set; }
    }
}
