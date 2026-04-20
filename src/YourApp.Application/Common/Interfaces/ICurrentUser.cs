namespace YourApp.Application.Common.Interfaces;

public interface ICurrentUser
{
    string? Id { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}
