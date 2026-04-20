using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using YourApp.Application.Common.Interfaces;

namespace YourApp.Infrastructure.Security;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Lấy UserId từ claim NameIdentifier hoặc sub
    public string? Id => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) 
                         ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
