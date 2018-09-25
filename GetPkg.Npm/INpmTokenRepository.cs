using System.Threading.Tasks;

namespace GetPkg.Npm {
    public interface INpmTokenRepository {
        Task<Token> CreateAuthenticatedTokenAsync(long userId);
        Task<Token> CreateAnonymousTokenAsync();
        Task<Token> GetByTokenIdAsync(string tokenId);
        Task AuthenticateTokenAsync(string tokenId, long userId);
    }
}
