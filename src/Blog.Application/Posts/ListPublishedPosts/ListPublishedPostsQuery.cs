namespace Blog.Application.Posts.ListPublishedPosts;

using Blog.Application.Common;

public record ListPublishedPostsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Category = null,
    string? Language = null);

public record PostSummaryDto(
    Guid Id,
    string Title,
    string Slug,
    string Language,
    string Summary,
    string? CoverImage,
    int ReadingTimeMinutes,
    DateTime? PublishedAt,
    string CategoryName,
    string CategorySlug);