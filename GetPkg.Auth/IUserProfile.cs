namespace GetPkg.Auth {
    public interface IUserProfile {
        string Id { get; }
        string Email { get; }
        string RefreshToken { get; }
    }

    public class UserProfile : IUserProfile {
        public string Id { get; }
        public string Email { get; }
        public string RefreshToken { get; }

        public UserProfile(string id, string email, string refreshToken) {
            Id = id;
            Email = email;
            RefreshToken = refreshToken;
        }
    }
}
