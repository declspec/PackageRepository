namespace GetPkg.Auth.V2.Credentials {
    public interface ICredentialsAuthorizationRequest {
        string Username { get; }
        string Password { get; }
    }

    public class CredentialsAuthorizationRequest : ICredentialsAuthorizationRequest {
        public string Username { get; }
        public string Password { get; }

        public CredentialsAuthorizationRequest(string username, string password) {
            Username = username;
            Password = password;
        }
    }
}
