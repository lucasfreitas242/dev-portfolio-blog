namespace Blog.Application.Posts.GetPostBySlug;

public record GetPostBySlugQuery(string Slug);

public record PostDetailDto(
    Guid Id,
    string Title,
    string Slug,
    string Summary,
    string ContentMd,
    string? CoverImage,
    int ReadingTimeMinutes,
    DateTime? PublishedAt,
    string AuthorName,
    string CategoryName,
    string CategorySlug,
    IReadOnlyList<string> Tags);