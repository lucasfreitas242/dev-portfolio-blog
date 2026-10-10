namespace Blog.Api.Endpoints;

using Blog.Application.Common;
using Blog.Application.Posts.CreatePost;
using Blog.Application.Posts.GetDraftById;
using Blog.Application.Posts.GetPostBySlug;
using Blog.Application.Posts.ListDraftPosts;
using Blog.Application.Posts.ListPublishedPosts;
using Blog.Application.Posts.PublishPost;
using Blog.Application.Posts.UpdatePost;

public static class PostEndpoints
{
    public static RouteGroupBuilder MapPostEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/posts")
                       .WithTags("Posts");

        group.MapGet("/", ListPublishedPosts)
             .WithName("ListPublishedPosts")
             .Produces<PagedResult<PostSummaryDto>>(StatusCodes.Status200OK);

        group.MapGet("/drafts", ListDraftPosts)
             .WithName("ListDraftPosts")
             .RequireAuthorization()
             .Produces<PagedResult<DraftSummaryDto>>(StatusCodes.Status200OK)
             .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/drafts/{id:guid}", GetDraftById)
             .WithName("GetDraftById")
             .RequireAuthorization()
             .Produces<DraftDetailDto>(StatusCodes.Status200OK)
             .ProducesProblem(StatusCodes.Status401Unauthorized)
             .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{slug}", GetPostBySlug)
             .WithName("GetPostBySlug")
             .Produces<PostDetailDto>(StatusCodes.Status200OK)
             .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreatePost)
             .WithName("CreatePost")
             .RequireAuthorization()
             .Produces<PostCreatedDto>(StatusCodes.Status201Created)
             .ProducesProblem(StatusCodes.Status400BadRequest)
             .ProducesProblem(StatusCodes.Status401Unauthorized)
             .ProducesProblem(StatusCodes.Status404NotFound)
             .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", UpdatePost)
             .WithName("UpdatePost")
             .RequireAuthorization()
             .Produces<PostUpdatedDto>(StatusCodes.Status200OK)
             .ProducesProblem(StatusCodes.Status400BadRequest)
             .ProducesProblem(StatusCodes.Status401Unauthorized)
             .ProducesProblem(StatusCodes.Status404NotFound)
             .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/publish", PublishPost)
             .WithName("PublishPost")
             .RequireAuthorization()
             .Produces<PostPublishedDto>(StatusCodes.Status200OK)
             .ProducesProblem(StatusCodes.Status401Unauthorized)
             .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> ListPublishedPosts(
        [AsParameters] ListPublishedPostsQuery query,
        ListPublishedPostsHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(query, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> ListDraftPosts(
        [AsParameters] ListDraftPostsQuery query,
        ListDraftPostsHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(query, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetDraftById(
        Guid id,
        GetDraftByIdHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetDraftByIdQuery(id), ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetPostBySlug(
        string slug,
        GetPostBySlugHandler handler,
        CancellationToken ct)
    {
        var post = await handler.HandleAsync(new GetPostBySlugQuery(slug), ct);
        return Results.Ok(post);
    }

    private static async Task<IResult> CreatePost(
        CreatePostCommand command,
        CreatePostHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(command, ct);
        return Results.Created($"/api/v1/posts/{result.Slug}", result);
    }

    private static async Task<IResult> UpdatePost(
        Guid id,
        UpdatePostCommand command,
        UpdatePostHandler handler,
        CancellationToken ct)
    {
        var updatedCommand = command with { Id = id };
        var result = await handler.HandleAsync(updatedCommand, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> PublishPost(
        Guid id,
        PublishPostHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new PublishPostCommand(id), ct);
        return Results.Ok(result);
    }
}