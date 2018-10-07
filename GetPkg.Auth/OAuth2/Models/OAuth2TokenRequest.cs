namespace GetPkg.Auth.OAuth2.Models {
    public class OAuth2TokenRequest {
        public string GrantType { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Code { get; set; }
        public string RedirectUri { get; set; }
    }
}
