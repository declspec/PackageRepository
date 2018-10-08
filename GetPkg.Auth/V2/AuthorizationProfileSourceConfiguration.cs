using System;

namespace GetPkg.Auth.V2 {
    public interface IAuthorizationProfileSourceConfiguration {
        Uri Endpoint { get; }
        string IdPath { get; }
        string EmailPath { get; }
    }

    public class AuthorizationProfileSourceConfiguration : IAuthorizationProfileSourceConfiguration {
        public Uri Endpoint { get; set; }
        public string IdPath { get; set; }
        public string EmailPath { get; set; }
    }
}
