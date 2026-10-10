namespace Blog.Application.Posts.ListDraftPosts;

using Blog.Application.Common;
using Blog.Domain.Interfaces;

public class ListDraftPostsHandler(IPostRepository postRepository)
{
    private const int MaxPageSize = 50;

    public async Task<PagedResult<DraftSummaryDto>> HandleAsync(
        ListDraftPostsQuery query, 
        CancellationToken ct = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        var drafts = await postRepository.GetDraftPostsAsync(page, pageSize, query.Category, query.Language, ct);
        var totalItems = await postRepository.CountDraftPostsAsync(query.Category, query.Language, ct);

        var items = drafts.Select(p => new DraftSummaryDto(
            Id: p.Id,
            Title: p.Title,
            Slug: p.Slug,
            Language: p.Language,
            Summary: p.Summary,
            CoverImage: p.CoverImage,
            ReadingTimeMinutes: p.ReadingTimeMinutes,
            Status: p.Status.ToString(),
            CreatedAt: p.CreatedAt,
            UpdatedAt: p.UpdatedAt,
            CategoryName: p.Category.Name,
            CategorySlug: p.Category.Slug
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PagedResult<DraftSummaryDto>(
            Items: items,
            Page: page,
            PageSize: pageSize,
            TotalItems: totalItems,
            TotalPages: totalPages
        );
    }
}
