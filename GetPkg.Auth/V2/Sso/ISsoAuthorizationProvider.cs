using System;
using System.Threading.Tasks;

namespace GetPkg.Auth.V2.Sso {
    public interface ISsoAuthorizationProvider {
        Task<Uri> GetAuthorizationUriAsync(ISsoAuthorizationConfiguration configuration, ISsoAuthorizationRequest request);
        Task<IAuthorizationProfile> CompleteAuthorizationAsync(ISsoAuthorizationConfiguration configuration, ISsoAuthorizationRequest request, ISsoAuthorizationResponse response)
    }
}
