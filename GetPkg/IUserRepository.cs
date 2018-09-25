using System.Threading.Tasks;

namespace GetPkg {
    public interface IUserRepository {
        Task<User> GetAsync(string id);
    }
}
