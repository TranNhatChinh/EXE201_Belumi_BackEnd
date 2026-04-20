using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using YourApp.Application.Features.Auth.Register;

namespace YourApp.API.Endpoints
{
    public static class AuthEndpoints
    {
        public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/auth").WithTags("Auth");

            group.MapPost("/register", async (RegisterCommand command, IMediator mediator) =>
            {
                // Note: The ValidationBehavior will handle throws if validation fails.
                // Assuming we have a global exception handler for ConflictException and ValidationException.
                var response = await mediator.Send(command);

                return Results.Created($"/users/{response.Id}", response);
            })
            .WithName("RegisterUser")
            .Produces<object>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict);

            return app;
        }
    }
}
