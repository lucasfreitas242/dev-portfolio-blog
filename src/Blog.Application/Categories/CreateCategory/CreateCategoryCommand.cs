namespace Blog.Application.Categories.CreateCategory;

public record CreateCategoryCommand(
    string Name,
    string? Slug = null,
    bool IsFeatured = false);

public record CategoryCreatedDto(
    Guid Id,
    string Name,
    string Slug,
    bool IsFeatured);
