using System;

namespace GetPkg.Auth {
    public interface IUserProfileSourceConfiguration {
        Uri Endpoint { get; }
        string IdPath { get; }
        string EmailPath { get; }
    }

    public class UserProfileConfiguration : IUserProfileSourceConfiguration {
        public Uri Endpoint { get; set; }
        public string IdPath { get; set; }
        public string EmailPath { get; set; }
    }
}
