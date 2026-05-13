using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Http;

namespace YourApp.Infrastructure.Middleware
{
    /// <summary>
    /// Firebase Auth Middleware - verify Firebase token và inject UserId + Email vào HttpContext.Items.
    /// Logic từ BE, thay thế toàn bộ JWT auth cũ.
    /// </summary>
    public class FirebaseAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public FirebaseAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"]
                .FirstOrDefault();

            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length);

                try
                {
                    var decodedToken =
                        await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);

                    context.Items["UserId"] = decodedToken.Uid;
                    context.Items["Email"]  = decodedToken.Claims["email"]?.ToString();
                }
                catch
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Invalid Firebase Token");
                    return;
                }
            }

            await _next(context);
        }
    }
}
