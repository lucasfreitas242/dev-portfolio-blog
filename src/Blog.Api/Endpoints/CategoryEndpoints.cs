namespace Blog.Api.Endpoints;

using Blog.Application.Categories.CreateCategory;
using Blog.Application.Categories.GetCategoryBySlug;
using Blog.Application.Categories.ListCategories;

public static class CategoryEndpoints
{
    public static RouteGroupBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/categories")
                       .WithTags("Categories");

        group.MapGet("/", ListCategories)
             .WithName("ListCategories")
             .Produces<IReadOnlyList<CategorySummaryDto>>(StatusCodes.Status200OK);

        group.MapGet("/{slug}", GetCategoryBySlug)
             .WithName("GetCategoryBySlug")
             .Produces<CategoryDetailDto>(StatusCodes.Status200OK)
             .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateCategory)
             .WithName("CreateCategory")
             .Produces<CategoryCreatedDto>(StatusCodes.Status201Created)
             .ProducesProblem(StatusCodes.Status400BadRequest)
             .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }

    private static async Task<IResult> ListCategories(
        ListCategoriesHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetCategoryBySlug(
        string slug,
        GetCategoryBySlugHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetCategoryBySlugQuery(slug), ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateCategory(
        CreateCategoryCommand command,
        CreateCategoryHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(command, ct);
        return Results.Created($"/api/v1/categories/{result.Slug}", result);
    }
}
