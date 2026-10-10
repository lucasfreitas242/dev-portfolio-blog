namespace Blog.Application.Categories.ListCategories;

using Blog.Domain.Interfaces;

public class ListCategoriesHandler(ICategoryRepository categoryRepository)
{
    public async Task<IReadOnlyList<CategorySummaryDto>> HandleAsync(CancellationToken ct = default)
    {
        var categories = await categoryRepository.GetAllAsync(ct);
        var postCounts = await categoryRepository.GetPublishedPostCountsAsync(ct);

        return categories.Select(c => new CategorySummaryDto(
            Id: c.Id,
            Name: c.Name,
            Slug: c.Slug,
            IsFeatured: c.IsFeatured,
            PostCount: postCounts.GetValueOrDefault(c.Id, 0)
        )).ToList();
    }
}
