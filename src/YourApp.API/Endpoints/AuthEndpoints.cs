using MediatR;
using YourApp.Application.Features.Auth.Register;
using YourApp.Application.Features.Auth.Login;
using YourApp.Application.Features.Auth.Refresh;
using YourApp.Application.Features.Auth.Logout;
using YourApp.Application.Features.Auth.VerifyEmail;
using YourApp.Application.Features.Auth.ResendVerification;
using YourApp.Application.Features.Auth.GetMe;
using System.Security.Claims;

namespace YourApp.API.Endpoints
{
    public static class AuthEndpoints
    {
        public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/auth").WithTags("Auth");

            group.MapPost("/register", async (RegisterCommand command, IMediator mediator) =>
            {
                var response = await mediator.Send(command);
                return Results.Created($"/users/{response.Id}", response);
            })
            .WithName("RegisterUser")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapPost("/login", async (LoginCommand command, IMediator mediator) =>
            {
                var response = await mediator.Send(command);
                return Results.Ok(response);
            })
            .WithName("Login")
            .Produces<LoginResponseDTO>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/refresh", async (RefreshCommand command, IMediator mediator) =>
            {
                var response = await mediator.Send(command);
                return Results.Ok(response);
            })
            .WithName("RefreshToken")
            .Produces<RefreshResponseDTO>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/logout", async (LogoutCommand command, IMediator mediator) =>
            {
                await mediator.Send(command);
                return Results.NoContent();
            })
            .WithName("Logout")
            .RequireAuthorization() // Yêu cầu Access Token hợp lệ
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/verify-email", async (VerifyEmailCommand command, IMediator mediator) =>
            {
                var response = await mediator.Send(command);
                return Results.Ok(response);
            })
            .WithName("VerifyEmail")
            .Produces<VerifyEmailResponseDTO>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapPost("/resend-verification", async (ResendVerificationCommand command, IMediator mediator) =>
            {
                var response = await mediator.Send(command);
                return Results.Ok(response);
            })
            .WithName("ResendVerification")
            .Produces<ResendVerificationResponseDTO>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);


            group.MapGet("/me", async (HttpContext context, IMediator mediator) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? context.User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();
                var response = await mediator.Send(new GetMeQuery(userId));
                return Results.Ok(response);
            })
.RequireAuthorization()
.WithName("GetMe")
.Produces<GetMeResponseDTO>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized);
            return app;
        }
    }
}
