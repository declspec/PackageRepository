﻿namespace GetPkg.Auth.V2.Sso.OAuth2 {
    internal class OAuth2TokenResponse {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public long ExpiresIn { get; set; }
        public string RefreshToken { get; set; }
        public string Scope { get; set; }
    }
}
