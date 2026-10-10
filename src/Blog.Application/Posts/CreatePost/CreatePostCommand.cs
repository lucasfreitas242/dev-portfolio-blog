namespace Blog.Application.Posts.CreatePost;

public record CreatePostCommand(
    string Title,
    string Summary,
    string ContentMd,
    Guid? CategoryId = null,
    string? CategorySlug = null,
    string? Slug = null,
    string? Language = null,
    Guid? TranslationGroupId = null,
    string? CoverImage = null,
    IReadOnlyList<string>? Tags = null,
    bool PublishNow = false,
    Guid? AuthorId = null);

public record PostCreatedDto(
    Guid Id,
    string Slug,
    string Language,
    Guid TranslationGroupId,
    string Status,
    DateTime CreatedAt,
    DateTime? PublishedAt);
