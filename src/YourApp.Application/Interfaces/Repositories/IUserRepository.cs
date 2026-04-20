using System.Threading;
using System.Threading.Tasks;
using YourApp.Domain.Entities;

namespace YourApp.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
        Task<User?> GetByUserByRefreshTokenHashAsync(string hash, CancellationToken cancellationToken = default);
        Task AddRefreshTokenAsync(RefreshToken token, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
