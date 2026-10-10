namespace Blog.Application.Posts.UpdatePost;

public record UpdatePostCommand(
    Guid Id,
    string Title,
    string Summary,
    string ContentMd,
    Guid? CategoryId = null,
    string? CategorySlug = null,
    string? Slug = null,
    string? CoverImage = null,
    IReadOnlyList<string>? Tags = null);

public record PostUpdatedDto(
    Guid Id,
    string Title,
    string Slug,
    string Language,
    Guid TranslationGroupId,
    string Summary,
    string ContentMd,
    string? CoverImage,
    int ReadingTimeMinutes,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? PublishedAt,
    string CategoryName,
    string CategorySlug,
    IReadOnlyList<string> Tags);
