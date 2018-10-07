namespace GetPkg.Auth.OAuth2.Models {
    public class OAuth2TokenResponse {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public long ExpiresIn { get; set; }
        public string RefreshToken { get; set; }
        public string Scope { get; set; }
    }
}
