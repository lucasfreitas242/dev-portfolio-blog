namespace Blog.Api.Endpoints;

using Blog.Application.Auth.Login;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth")
                       .WithTags("Auth");

        group.MapPost("/login", Login)
             .WithName("Login")
             .Produces<LoginResultDto>(StatusCodes.Status200OK)
             .ProducesProblem(StatusCodes.Status400BadRequest)
             .ProducesProblem(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> Login(
        LoginCommand command,
        LoginHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(command, ct);
        return Results.Ok(result);
    }
}
