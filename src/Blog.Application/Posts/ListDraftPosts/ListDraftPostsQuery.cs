namespace Blog.Application.Posts.ListDraftPosts;

using Blog.Application.Common;

public record ListDraftPostsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Category = null,
    string? Language = null);

public record DraftSummaryDto(
    Guid Id,
    string Title,
    string Slug,
    string Language,
    string Summary,
    string? CoverImage,
    int ReadingTimeMinutes,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string CategoryName,
    string CategorySlug);
