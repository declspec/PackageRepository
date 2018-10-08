using System.Threading.Tasks;

namespace GetPkg.Auth.V2.Credentials {
    public interface ICredentialsAuthorizationProvider {
        Task<IAuthorizationProfile> AuthorizeAsync(ICredentialsAuthorizationConfiguration config, ICredentialsAuthorizationRequest request);
    }
}
