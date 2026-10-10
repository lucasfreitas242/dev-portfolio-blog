namespace Blog.Application.Posts.GetDraftById;

public record GetDraftByIdQuery(Guid Id);

public record DraftDetailDto(
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
    string AuthorName,
    string CategoryName,
    string CategorySlug,
    IReadOnlyList<string> Tags);
