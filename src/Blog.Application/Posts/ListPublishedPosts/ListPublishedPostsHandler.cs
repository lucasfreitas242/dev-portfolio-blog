namespace Blog.Application.Posts.ListPublishedPosts;

using Blog.Domain.Interfaces;

public class ListPublishedPostsHandler(IPostRepository postRepository)
{
    private const int MaxPageSize = 50;

    public async Task<PagedResult<PostSummaryDto>> HandleAsync(
        ListPublishedPostsQuery query, 
        CancellationToken ct = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        var postsTask = postRepository.GetPublishedPostsAsync(page, pageSize, query.Category, ct);
        var totalCountTask = postRepository.CountPublishedPostsAsync(query.Category, ct);

        await Task.WhenAll(postsTask, totalCountTask);

        var posts = await postsTask;
        var totalItems = await totalCountTask;

        var items = posts.Select(p => new PostSummaryDto(
            p.Id,
            p.Title,
            p.Slug,
            p.Summary,
            p.CoverImage,
            p.ReadingTimeMinutes,
            p.PublishedAt,
            p.Category.Name,
            p.Category.Slug
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PagedResult<PostSummaryDto>(
            Items: items,
            Page: page,
            PageSize: pageSize,
            TotalItems: totalItems,
            TotalPages: totalPages
        );
    }
}