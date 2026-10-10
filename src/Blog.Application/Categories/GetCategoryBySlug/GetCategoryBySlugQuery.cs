namespace Blog.Application.Categories.GetCategoryBySlug;

public record GetCategoryBySlugQuery(string Slug);

public record CategoryDetailDto(
    Guid Id,
    string Name,
    string Slug,
    bool IsFeatured,
    int PostCount);
