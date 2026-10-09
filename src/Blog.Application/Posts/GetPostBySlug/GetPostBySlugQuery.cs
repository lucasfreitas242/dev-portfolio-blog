namespace Blog.Application.Posts.GetPostBySlug;

public record GetPostBySlugQuery(string Slug);

public record PostTranslationDto(
    string Language,
    string Slug);

public record PostDetailDto(
    Guid Id,
    string Title,
    string Slug,
    string Language,
    Guid TranslationGroupId,
    string Summary,
    string ContentMd,
    string? CoverImage,
    int ReadingTimeMinutes,
    DateTime? PublishedAt,
    string AuthorName,
    string CategoryName,
    string CategorySlug,
    IReadOnlyList<string> Tags,
    IReadOnlyList<PostTranslationDto> AvailableTranslations);