namespace Blog.Api.Endpoints;

using Blog.Application.Posts.GetPostBySlug;
using Blog.Application.Posts.ListPublishedPosts;

public static class PostEndpoints
{
    public static RouteGroupBuilder MapPostEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/posts")
                       .WithTags("Posts");

        group.MapGet("/", ListPublishedPosts)
             .WithName("ListPublishedPosts")
             .Produces<PagedResult<PostSummaryDto>>(StatusCodes.Status200OK);

        group.MapGet("/{slug}", GetPostBySlug)
             .WithName("GetPostBySlug")
             .Produces<PostDetailDto>(StatusCodes.Status200OK)
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

    private static async Task<IResult> GetPostBySlug(
        string slug,
        GetPostBySlugHandler handler,
        CancellationToken ct)
    {
        var post = await handler.HandleAsync(new GetPostBySlugQuery(slug), ct);
        return Results.Ok(post);
    }
}