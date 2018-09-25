using System.Threading.Tasks;

namespace GetPkg {
    public interface ITokenRepository {
        Task<Token> GetAsync(string tokenId);
        Task<Token> CreateAsync();
        Task<Token> CreateAsync(string userId);
        Task AuthenticateAsync(Token token);
    }
}
