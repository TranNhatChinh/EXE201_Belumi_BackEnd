using System.Threading;
using System.Threading.Tasks;
using YourApp.Domain.Entities;

namespace YourApp.Application.Interfaces.Repositories
{
    /// <summary>
    /// Chỉ giữ operations cần thiết sau khi Firebase xử lý auth.
    /// </summary>
    public interface IUserRepository
    {
        Task<User?> GetByFirebaseUidAsync(string firebaseUid, CancellationToken cancellationToken = default);
        Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
