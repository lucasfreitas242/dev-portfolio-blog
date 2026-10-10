namespace Blog.Application.Categories.ListCategories;

public record CategorySummaryDto(
    Guid Id,
    string Name,
    string Slug,
    bool IsFeatured,
    int PostCount);
