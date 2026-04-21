using YourApp.Domain.Entities;

namespace YourApp.Application.Common.Interfaces;

public interface IMailService
{
    Task SendVerificationAsync(User user, string token);
}
